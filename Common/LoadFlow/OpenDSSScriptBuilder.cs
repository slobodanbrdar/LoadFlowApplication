using Common.Logger;
using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using Common.TopologyService;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Common.LoadFlow
{
	public class OpenDSSScriptBuilder
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		private Dictionary<long, ResourceDescription> Resources { get; set; }
		private TopologyResult Topology { get; set; }
		private ModelResourcesDesc modelResourcesDesc;
		private bool rootInitialized = false;
		public OpenDSSScriptBuilder(Dictionary<long, ResourceDescription> resources, TopologyResult topology)
		{
			Resources = resources;
			Topology = topology;
			modelResourcesDesc = new ModelResourcesDesc();
		}

		public string GenerateDSSScript()
		{
			Logger.LogInformation($"Generating dss script for root 0x{Topology.RootId:X16} started.");
			string dssScript = null;
			if (Resources == null || Topology == null)
			{
				return dssScript;
			}

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Clear{Environment.NewLine}");


			for (int i = 0; i < Topology.Branches.Count; i++)
			{
				if (Topology.Branches[i].OwnerCircuit[0] != Topology.RootId)
				{
					continue;
				}
				ResourceDescription rd = null;
				if(Topology.Branches[i] is MPConnectivityBranch connectivityBranch && Topology.Branches[i + 1] is MPConnectivityBranch connectivityBranch2)
				{
					rd = Resources[connectivityBranch.OriginalBranchLid];
					GenerateScriptLineForElement(rd, stringBuilder, connectivityBranch, connectivityBranch2);
					i++;
					continue;
				}
				else if (Topology.Branches[i] is MPConnectivityBranch && !(Topology.Branches[i + 1] is MPConnectivityBranch))
				{
					string message = $"Branch with lid {Topology.Branches[i]} is connectivity branch, but next branch is not.";
					Logger.LogError(message);
					throw new Exception(message);
				}

				rd = Resources[Topology.Branches[i].Lid];
				GenerateScriptLineForElement(rd, stringBuilder, Topology.Branches[i]);
			}

			if (!rootInitialized)
			{
				string message = "Initializing of open dss script failed becasue root was not initialized.";
				Logger.LogError(message);
				throw new Exception(message);
			}


			dssScript = stringBuilder.ToString();
			Logger.LogInformation($"Generating dss script for root 0x{Topology.RootId:X16} finished.");
			return dssScript;
		}

		private void GenerateScriptLineForElement(ResourceDescription rd, StringBuilder sb, MPBranch branch, MPBranch branch2 = null)
		{
			long gid = rd.Id;
			DMSType elementType = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(gid);

			switch (elementType)
			{
				case DMSType.ENERGYCONSUMER:
					GenerateScriptLineForEnergyConsumer(rd, branch, sb);
					break;
				case DMSType.ENERGYSOURCE:
					GenerateScriptLineForRoot(rd, branch, sb);
					break;
				case DMSType.POWERTRANSFORMER:
					GenerateScriptLineForTransformer(rd, branch, sb);
					break;
				case DMSType.SWITCH:
					GenerateScriptLineForSwitch(rd, (MPConnectivityBranch)branch, (MPConnectivityBranch)branch2, sb);
					break;
				case DMSType.ACLINESEGMENT:
					GenerateScriptLineForLineSegment(rd, branch, sb);
					break;
				default:
					Logger.LogWarning($"Non branch type: {elementType} for gid: {gid:x16}");
					break;
			}
		}

		private void GenerateScriptLineForRoot(ResourceDescription rootElement, MPBranch branch, StringBuilder sb)
		{
			if (rootElement.Id != Topology.RootId) //Nije odgovarajuci root
			{
				return;
			}

			string rootMrid = rootElement.GetProperty(ModelCode.IDOBJ_MRID).AsString();
			long rootBVGid = rootElement.GetProperty(ModelCode.CONDEQ_BASVOLTAGE).AsReference();
			ResourceDescription baseVoltageRD = Resources[rootBVGid];

			float baseVoltage = baseVoltageRD.GetProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE).AsFloat() / 1000;
			float angle = rootElement.GetProperty(ModelCode.ENERGYSOURCE_VOLTAGEANGLE).AsFloat();
			float x = rootElement.GetProperty(ModelCode.ENERGYSOURCE_X).AsFloat();
			float x0 = rootElement.GetProperty(ModelCode.ENERGYSOURCE_X0).AsFloat();
			float r = rootElement.GetProperty(ModelCode.ENERGYSOURCE_R).AsFloat();
			float r0 = rootElement.GetProperty(ModelCode.ENERGYSOURCE_R0).AsFloat();


			long nodeId = branch.DownNode[0];
			ResourceDescription connNode = Resources[nodeId];

			string connNodeMRID = connNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			sb.AppendLine($"new object=circuit.{rootMrid} basekv={baseVoltage} pu=1 angle={angle} frequency=60.0 R0={r0} R1={r} X0={x0} X={x} bus2={connNodeMRID}");

			rootInitialized = true;
		}

		private void GenerateScriptLineForLineSegment(ResourceDescription acLineSegmentElement, MPBranch branch, StringBuilder sb)
		{
			string acLineSegmentMrid = acLineSegmentElement.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			long downNodeGid = branch.DownNode[0];

			long upNodeGid = branch.UpNode[0];

			if (!Resources.ContainsKey(downNodeGid) || !Resources.ContainsKey(upNodeGid))
			{
				string message = "Gid of up node or down node cannot be found in network model.";
				Logger.LogError(message);
				throw new Exception(message);
			}

			ResourceDescription downNode = Resources[downNodeGid];
			ResourceDescription upNode = Resources[upNodeGid];

			string downNodeMrid = downNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();
			string upNodeMrid = upNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			float length = acLineSegmentElement.GetProperty(ModelCode.CONDUCTOR_LENGTH).AsFloat();

			float r = acLineSegmentElement.GetProperty(ModelCode.ACLINESEGMENT_R).AsFloat() / length; //jer je per unit
			float r0 = acLineSegmentElement.GetProperty(ModelCode.ACLINESEGMENT_R0).AsFloat() / length; 
			float x = acLineSegmentElement.GetProperty(ModelCode.ACLINESEGMENT_X).AsFloat() / length; 
			float x0 = acLineSegmentElement.GetProperty(ModelCode.ACLINESEGMENT_X0).AsFloat() / length;

			List<long> terminalGids = acLineSegmentElement.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();

			List<ResourceDescription> terminals = new List<ResourceDescription>();
			foreach (long terminalGid in terminalGids)
			{
				if (!Resources.ContainsKey(terminalGid))
				{
					string message = $"Terminal with gid 0x{terminalGid:X16} cannot be found in the network model.";
					Logger.LogError(message);
					throw new Exception(message);
				}

				terminals.Add(Resources[terminalGid]);
			}

			int numberOfPhases = GetPhaseNumber(terminals);

			sb.AppendLine($"new Line.{acLineSegmentMrid} bus1={upNodeMrid} bus2={downNodeMrid} phases={numberOfPhases} length={length} units=Ft r={r} r0={r0} x={x} x0={x0}");


		}

		private void GenerateScriptLineForSwitch(ResourceDescription switchElement, MPConnectivityBranch branch, MPConnectivityBranch branch2, StringBuilder sb)
		{
			string switchMrid = switchElement.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			long upNodeGid = branch2.UpNode[0];

			long downNodeGid = branch.DownNode[0];

			if (!Resources.ContainsKey(downNodeGid) || !Resources.ContainsKey(upNodeGid))
			{
				string message = "Gid of up node or down node cannot be found in network model.";
				Logger.LogError(message);
				throw new Exception(message);
			}

			ResourceDescription downNode = Resources[downNodeGid];
			ResourceDescription upNode = Resources[upNodeGid];

			string downNodeMrid = downNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();
			string upNodeMrid = upNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			string normalOpen = switchElement.GetProperty(ModelCode.SWITCH_NORMALOPEN).AsBool() ? "Open" : "Closed";
			string currentState = switchElement.GetProperty(ModelCode.SWITCH_NORMALOPEN).AsBool() ? "Open" : "Closed";

			List<long> terminalGids = switchElement.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();
			List<ResourceDescription> terminals = new List<ResourceDescription>();
			foreach (long terminalGid in terminalGids)
			{
				if (!Resources.ContainsKey(terminalGid))
				{
					string message = $"Terminal with gid 0x{terminalGid:X16} cannot be found in the network model.";
					Logger.LogError(message);
					throw new Exception(message);
				}

				terminals.Add(Resources[terminalGid]);
			}

			int numberOfPhases = GetPhaseNumber(terminals);

			sb.AppendLine($"new Line.{switchMrid} bus1={upNodeMrid} bus2={downNodeMrid} Switch=y");
			sb.AppendLine($"new SwtControl.{switchMrid}Swt SwitchedObj=Line.{switchMrid} Normal={normalOpen} State={currentState}");

		}

		private void GenerateScriptLineForEnergyConsumer(ResourceDescription energyConsumerElement, MPBranch branch, StringBuilder sb)
		{
			string energyConumerMrid = energyConsumerElement.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			long upNodeGid = branch.UpNode[0];

			if (!Resources.ContainsKey(upNodeGid))
			{
				string message = "Gid of up node cannot be found in network model.";
				Logger.LogError(message);
				throw new Exception(message);
			}

			ResourceDescription upNode = Resources[upNodeGid];

			string upNodeMrid = upNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			long baseVoltegeGid = energyConsumerElement.GetProperty(ModelCode.CONDEQ_BASVOLTAGE).AsReference();
			if (!Resources.ContainsKey(baseVoltegeGid))
			{
				string message = $"Base voltage with gid 0x{baseVoltegeGid:X16} cannot be found in network model.";
				Logger.LogError(message);
				throw new Exception(message);
			}
			ResourceDescription baseVoltage = Resources[baseVoltegeGid];

			float kv = baseVoltage.GetProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE).AsFloat() / 1000;

			float kw = energyConsumerElement.GetProperty(ModelCode.ENERGYCONSUMER_PFIXED).AsFloat() / 1000;
			float kvar = energyConsumerElement.GetProperty(ModelCode.ENERGYCONSUMER_QFIXED).AsFloat() / 1000;

			List<long> terminalGids = energyConsumerElement.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();

			List<ResourceDescription> terminals = new List<ResourceDescription>();
			foreach (long terminalGid in terminalGids)
			{
				if (!Resources.ContainsKey(terminalGid))
				{
					string message = $"Terminal with gid 0x{terminalGid:X16} cannot be found in the network model.";
					Logger.LogError(message);
					throw new Exception(message);
				}

				terminals.Add(Resources[terminalGid]);
			}

			int numberOfPhases = GetPhaseNumber(terminals);

			PhaseShuntConnectionKind connKind = (PhaseShuntConnectionKind)energyConsumerElement.GetProperty(ModelCode.ENERGYCONSUMER_PHASECONNECTION).AsEnum();
			string connKindString = GetStringFromShunConnectionKind(connKind);


			sb.AppendLine($"new Load.{energyConumerMrid} bus1={upNodeMrid} phases={numberOfPhases} kV={kv} kW={kw} kvar={kvar} conn={connKindString}");
		}

		private void GenerateScriptLineForTransformer(ResourceDescription transformerElement, MPBranch branch, StringBuilder sb)
		{
			List<long> pTransformerEndGids = transformerElement.GetProperty(ModelCode.POWERTRANSFORMER_TRANSFORMERENDS).AsReferences();

			if (pTransformerEndGids.Count != 2) //Error if less than 1 or more than 3 windings
			{
				Logger.LogError($"Transfoemer with gid: 0x{transformerElement.Id:X16} has {pTransformerEndGids.Count}. Only 2 windings transformers is supported.");
				return;
			}
			
			
			List<ResourceDescription> powerTransformerEnds = new List<ResourceDescription>();

			foreach (long gid in pTransformerEndGids)
			{
				if (!Resources.ContainsKey(gid))
				{
					Logger.LogError($"Power transformer end with gid 0x{gid:X16} is not in the network model.");
					return;
				}

				powerTransformerEnds.Add(Resources[gid]);
			}

			powerTransformerEnds = powerTransformerEnds.OrderBy(key => key.GetProperty(ModelCode.PTRANSFORMEREND_ENDNUMBER).AsInt()).ToList();

			string transformerMRID = transformerElement.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			List<ResourceDescription> terminals = GetTerminalsForConductingEquipmentElement(transformerElement);

			int numberOfPhases = GetPhaseNumber(terminals);

			if (numberOfPhases < 1)
			{
				Logger.LogError("Error with number of phases");
			}

			string buses = AppendBusesForTransformer(transformerElement, branch, pTransformerEndGids.Count);
			if (buses == null)
			{
				Logger.LogError($"Error while appending buses for transformer with gid: 0x{transformerElement.Id:X16}");
			}


			sb.Append($"new Transformer.{transformerMRID} windings={pTransformerEndGids.Count} phases={numberOfPhases} buses={buses} ");
			AppendWindingsInfo(sb, powerTransformerEnds, transformerElement, branch);
			
		}


		private string AppendBusesForTransformer(ResourceDescription element, MPBranch branch, int numberOfWindings)
		{
			string buses = "";

			long downNodeGid = branch.DownNode[0];

			long upNodeGid = branch.UpNode[0];

			if (!Resources.ContainsKey(downNodeGid) || !Resources.ContainsKey(upNodeGid))
			{
				Logger.LogError("Gid of up node or down node cannot be found in network model.");
				return null;
			}

			ResourceDescription downNode = Resources[downNodeGid];
			ResourceDescription upNode = Resources[upNodeGid];

			string downNodeMrid = downNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();
			string upNodeMrid = upNode.GetProperty(ModelCode.IDOBJ_MRID).AsString();

			buses += $"[{upNodeMrid} ";

			for (int i = 1; i < numberOfWindings; i++)
			{
				buses += $"{downNodeMrid}";

				if (i < numberOfWindings - 1)
				{
					buses += " ";
				}
			}

			buses += "]";
			return buses;
		}

		private void AppendWindingsInfo(StringBuilder sb, List<ResourceDescription> powerTransformerEnds, ResourceDescription powerTransformer, MPBranch transformerBranch)
		{
			string kvas = "[";
			string kvs = "[";
			string connections = "[";


			foreach (ResourceDescription powerTransformerEnd in powerTransformerEnds)
			{
				float kva = powerTransformerEnd.GetProperty(ModelCode.PTRANSFORMEREND_RATEDS).AsFloat() / 1000;
				float kv = powerTransformerEnd.GetProperty(ModelCode.PTRANSFORMEREND_RATEDU).AsFloat() / 1000;
				WindingConnection windingConnection = (WindingConnection)powerTransformerEnd.GetProperty(ModelCode.PTRANSFORMEREND_CONNKIND).AsEnum();
				string windingConnectionString = GetConnectionStringFromWindingConnection(windingConnection);

				kvas += $"{kva} ";
				kvs += $"{kv} ";
				connections += $"{windingConnectionString} ";
			}

			kvas += "]";
			kvs += "]";
			connections += "]";

			string rpu = "";
			string xpu = "";

			if (powerTransformerEnds.Count == 2)
			{
				float ratedS1 = powerTransformerEnds[0].GetProperty(ModelCode.PTRANSFORMEREND_RATEDS).AsFloat();
				float ratedU1 = powerTransformerEnds[0].GetProperty(ModelCode.PTRANSFORMEREND_RATEDU).AsFloat();

				float r1 = powerTransformerEnds[0].GetProperty(ModelCode.PTRANSFORMEREND_R).AsFloat();
				float x1 = powerTransformerEnds[0].GetProperty(ModelCode.PTRANSFORMEREND_X).AsFloat();

				Complex z1 = new Complex(r1, x1);

				float zBase = (ratedU1 * ratedU1) / ratedS1;

				Complex zPu = z1 / zBase;

				rpu = (zPu.Real * 100).ToString();
				xpu = (zPu.Imaginary * 100).ToString();

			}
			else if(powerTransformerEnds.Count == 3)
			{
				throw new NotImplementedException("Three winding transformer is not supported.");
			}

			sb.AppendLine($"kvas={kvas} kvs={kvs} conns={connections} %loadloss={rpu} XHL={xpu}");
		}



		private string GetConnectionStringFromWindingConnection(WindingConnection windingConnection)
		{
			string connectionKind = null;
			switch (windingConnection)
			{
				case WindingConnection.Y:
					connectionKind = "wye";
					break;
				case WindingConnection.D:
					connectionKind = "delta";
					break;
				case WindingConnection.Z:
				case WindingConnection.I:
				case WindingConnection.Scott:
				case WindingConnection.OY:
				case WindingConnection.OD:
				default:
					Logger.LogError("Not supported or unknown connection kind.");
					break;
			}

			return connectionKind;
		}

		private string GetStringFromShunConnectionKind(PhaseShuntConnectionKind phaseShuntConnectionKind)
		{
			string connKind = null;
			switch (phaseShuntConnectionKind)
			{
				case PhaseShuntConnectionKind.D:
					connKind = "delta";
					break;
				case PhaseShuntConnectionKind.Y:
					connKind = "wye";
					break;
				case PhaseShuntConnectionKind.I:
				case PhaseShuntConnectionKind.Yn:
				default:
					Logger.LogError("Not supporte or unknown shunt connection kind");
					break;
			}

			return connKind;
		}

		

		private int GetPhaseNumber(List<ResourceDescription> terminals)
		{
			int phaseNumber = -1;

			foreach (ResourceDescription terminal in terminals)
			{
				PhaseCode phaseCode = (PhaseCode)(terminal.GetProperty(ModelCode.TERMINAL_PHASES).AsEnum());
				int numberOfPhases = GetPhaseNumberFromPhaseCode(phaseCode);
				if (numberOfPhases == -1) //Ako ne postoji enum za fazu
				{
					break;
				}

				if (phaseNumber == -1) //Prvo dodeljivanje
				{
					phaseNumber = numberOfPhases;
				}
				else
				{
					if (phaseNumber != numberOfPhases) //Razlicit broj faza u odnosu na do sada
					{
						phaseNumber = -1;
						break;
					}
				}
			}


			return phaseNumber;

		}

		private int GetPhaseNumberFromPhaseCode(PhaseCode phaseCode)
		{
			int phaseNumber = -1;
			//TODO: check this
			switch (phaseCode)
			{
				case PhaseCode.Unknown:
				case PhaseCode.N:
					phaseNumber = 0;
					break;
				case PhaseCode.A:
				case PhaseCode.B:
				case PhaseCode.C:
				case PhaseCode.AN:
				case PhaseCode.CN:
				case PhaseCode.BN:
					phaseNumber = 1;
					break;
				case PhaseCode.AB:
				case PhaseCode.AC:
				case PhaseCode.BC:
				case PhaseCode.BCN:
				case PhaseCode.ACN:
				case PhaseCode.ABN:
					phaseNumber = 2;
					break;
				case PhaseCode.ABC:
				case PhaseCode.ABCN:
					phaseNumber = 3;
					break;
				default:
					Logger.LogWarning("Unknown phase code.");
					break;
			}

			return phaseNumber;
		}

		private List<ResourceDescription> GetTerminalsForConductingEquipmentElement(ResourceDescription element)
		{
			List<ResourceDescription> terminals = new List<ResourceDescription>();
			List<long> terminalGids = element.GetProperty(ModelCode.CONDEQ_TERMINALS).AsReferences();

			foreach (long terminalGid in terminalGids)
			{
				if (Resources.ContainsKey(terminalGid))
				{
					terminals.Add(Resources[terminalGid]);
				}
				else
				{
					Logger.LogError($"Terminal with gid 0x{terminalGid:X16} cannot be found in netrwork model");
					terminals = null;
					break;
				}
			}

			return terminals;
		}
	}
}
