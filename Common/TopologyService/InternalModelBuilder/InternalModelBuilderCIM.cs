using Common.Logger;
using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using Common.TopologyService.ExtendedClassess;
using Common.TopologyService.InternalModel.Circuts;
using Common.TopologyService.InternalModel.GraphElems;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Common.TopologyService.InternalModelBuilder
{
	public class InternalModelBuilderCIM
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		private CModelFramework internalModel;
		public CModelFramework InternalModel
		{
			get
			{
				if (IsInternalModelInitialized)
				{
					return internalModel;
				}
				else
				{
					Logger.LogWarning("Internal model is not initialized.");
					return null;
				}
			}
		}
		private bool isNodesInitialized;
		private bool isBranchesInitialized;
		private bool isRootsInitialized;
		private bool isSwitchesInitialized;

		public bool IsInternalModelInitialized
		{
			get { return isNodesInitialized && isBranchesInitialized && isRootsInitialized && isSwitchesInitialized; }
		}

		public InternalModelBuilderCIM(CModelFramework internalModel)
		{
			this.internalModel = internalModel;
			isNodesInitialized = false;
			isBranchesInitialized = false;
			isRootsInitialized = false;
			isSwitchesInitialized = false;
		}


		public void ReadSources(Dictionary<long, ResourceDescription> cimSources, Dictionary<long, ResourceDescription> cimTerminals)
		{

			Dictionary<long, MPNode> nodes = internalModel.Nodes;
			Dictionary<long, MPRoot> roots = internalModel.Roots;
			Dictionary<long, MPBranch> branches = internalModel.Branches;

			foreach (ResourceDescription source in cimSources.Values)
			{
				long rootId = source.Id;
				List<long> terminalsID = source.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();

				if (terminalsID.Count < 1)
				{
					string message = "Invalid CIM model. Root must have at least one terminal";
					Logger.LogError(message);
					throw new Exception(message); 
				}
				//TODO: svi treba da imaju istu faznost
				ResourceDescription firstTerminal = cimTerminals[terminalsID[0]];
				
				EPhaseCode phaseCode = (EPhaseCode)firstTerminal.GetProperty(ModelCode.TERMINAL_PHASES).AsEnum();
				if (nodes.ContainsKey(rootId))
				{
					string message = $"Bad input file. There is already root node with specific gid ({rootId:X16}).";
					Logger.LogError(message);
					throw new Exception(message);
				}

				if (!roots.ContainsKey(rootId))
				{
					MPRootNode rootNode = new MPRootNode(rootId, rootId, phaseCode);
					nodes.Add(rootId, rootNode);

					MPRoot root = new MPRoot(rootId, rootId);
					roots.Add(rootId, root);

					long rootConnNode = firstTerminal.GetProperty(ModelCode.TERMINAL_CONNECTIVITYNODE).AsReference();
					MPBranch rootBranch = new MPLine(rootId, phaseCode, rootId, rootConnNode);
					branches.Add(rootId, rootBranch);
				}
				
			}

			isRootsInitialized = true;
		}

		public void ReadConnectivityNodes(Dictionary<long, ResourceDescription> cimConnNodes, Dictionary<long, ResourceDescription> cimTerminals)
		{
			Dictionary<long, MPNode> nodes = internalModel.Nodes;


			foreach (ResourceDescription node in cimConnNodes.Values)
			{
				if (!nodes.ContainsKey(node.Id))
				{
					//TODO: svi treba da imaju istu faznost
					List<long> terminalsID = node.GetProperty(ModelCode.CONNECTIVITYNODE_TERMINALS).AsReferences();

					if (terminalsID.Count < 1)
					{
						string message = "Invalid CIM model. Connectivity node must have at least one terminal";
						Logger.LogError(message);
						throw new Exception(message);
					}
					ResourceDescription firstTerminal = cimTerminals[terminalsID[0]];
					EPhaseCode phaseCode = (EPhaseCode)firstTerminal.GetProperty(ModelCode.TERMINAL_PHASES).AsEnum();

					MPBusNode newNode = new MPBusNode(node.Id, phaseCode);

					nodes.Add(node.Id, newNode);
				}
				else
				{
					string errMessage = $"Invalid CIM model. There is already node with same key: {node.Id:X16}";
					Logger.LogError(errMessage);
					throw new Exception(errMessage);
				}
				
			}

			isNodesInitialized = true;
		}

		public void ReadBranches(Dictionary<long, ResourceDescription> cimBranches, Dictionary<long, ResourceDescription> cimTerminals)
		{
			Dictionary<long, MPBranch> branches = internalModel.Branches;
			Dictionary<long, MPNode> nodes = internalModel.Nodes;

			if (!isNodesInitialized)
			{
				return;
			}

			foreach (ResourceDescription cimBranch in cimBranches.Values)
			{
				if (!branches.ContainsKey(cimBranch.Id))
				{
					long branchId = cimBranch.Id;
					List<long> terminalsID = cimBranch.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();
					AddBranch(cimTerminals, branches, nodes, branchId, terminalsID);
				}
				else
				{
					string errMessage = $"Invalid CIM model. There is already branch with same key {cimBranch.Id:X16}";
					Logger.LogError(errMessage);
					throw new Exception(errMessage);
				}
			}

			isBranchesInitialized = true;
		}

		private void AddBranch(Dictionary<long, ResourceDescription> cimTerminals, Dictionary<long, MPBranch> branches, Dictionary<long, MPNode> nodes, long branchId, List<long> terminalsID)
		{
			if (terminalsID.Count < 1)
			{
				string message = "Invalid CIM model. Connectivity node must have at least one terminal";
				Logger.LogError(message);
				throw new Exception(message);
			}
			//TODO: za sada, slucaj kada ima vise
			ResourceDescription firstTerminal = cimTerminals[terminalsID[0]];
			ResourceDescription secondTerminal = cimTerminals[terminalsID[1]];

			EPhaseCode phaseCode = (EPhaseCode)firstTerminal.GetProperty(ModelCode.TERMINAL_PHASES).AsEnum();
			long firstNodeId = firstTerminal.GetProperty(ModelCode.TERMINAL_CONNECTIVITYNODE).AsLong();
			long secondNodeId = secondTerminal.GetProperty(ModelCode.TERMINAL_CONNECTIVITYNODE).AsLong();

			if (nodes.ContainsKey(firstNodeId) && nodes.ContainsKey(secondNodeId))
			{
				MPLine branch = new MPLine(branchId, phaseCode, firstNodeId, secondNodeId);

				branches.Add(branchId, branch);
			}
			else
			{
				string message = $"Invalid CIM model. There is(are) not node(s) with specific lid(s). First node: {firstNodeId}, Second node {secondNodeId}";
				Logger.LogError(message);
				throw new Exception(message);
			}
		}

		public void ReadSwitches(Dictionary<long, ResourceDescription> cimSwitches, Dictionary<long, ResourceDescription> cimTerminals)
		{
			if (!isRootsInitialized || !isBranchesInitialized || !isNodesInitialized)
			{
				return;
			}

			Dictionary<long, MPBranch> branches = internalModel.Branches;
			Dictionary<long, MPNode> nodes = internalModel.Nodes;
			Dictionary<long, MPSwitchDevice> switches = internalModel.SwitchDevices;

			foreach (ResourceDescription cimSwitch in cimSwitches.Values)
			{
				long switchId = cimSwitch.Id;
				if (!branches.ContainsKey(switchId))
				{
					List<long> terminalsID = cimSwitch.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();
					AddBranch(cimTerminals, branches, nodes, switchId, terminalsID);
					if (!switches.ContainsKey(switchId))
					{
						ResourceDescription firstTerminal = cimTerminals[terminalsID[0]];
						long nodeId = firstTerminal.GetProperty(ModelCode.TERMINAL_CONNECTIVITYNODE).AsReference();
						EPhaseCode switchPhases = (EPhaseCode)firstTerminal.GetProperty(ModelCode.TERMINAL_PHASES).AsEnum();
						EPhaseCode switchState = GetSwitchState(switchPhases, cimSwitch.GetProperty(ModelCode.SWITCH_NORMALOPEN).AsBool());

						MPSwitchDevice newSwitch = new MPSwitchDevice(switchId, switchId, nodeId, switchPhases, switchState);

						switches.Add(switchId, newSwitch);
					}
					else
					{
						string errMessage = $"Switch with id {switchId} already exists in intenal model.";
						Logger.LogError(errMessage);
						throw new Exception(errMessage);
					}
				}
				else
				{
					string errMessage = $"Switch (branch) with id {switchId} already exists in intenal model.";
					Logger.LogError(errMessage);
					throw new Exception(errMessage);
				}

			}

			isSwitchesInitialized = true;
		}

		private EPhaseCode GetSwitchState(EPhaseCode phaseCode, bool open)
		{
			EPhaseCode mask = open ? EPhaseCode.NONE : EPhaseCode.ABCN;

			return mask & phaseCode;
		}
	}
}
