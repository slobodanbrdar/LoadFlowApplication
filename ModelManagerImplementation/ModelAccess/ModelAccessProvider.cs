using Common;
using Common.Clients;
using Common.LoadFlow;
using Common.Logger;
using Common.ModelManagerInterfaces;
using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using Common.TopologyService;
using Common.TopologyService.InternalModelBuilder;
using Common.TopologyServiceInterfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModelManagerImplementation.ModelAccess
{
	public class ModelAccessProvider : IModelAccessContract
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private ITopologyAnalyzer topologyAnalyzer;
		private ITopologyAnalyzer TopologyAnalyzer
		{
			get
			{
				return TopologyAnalyzerClient.CreateClient();
			}
		}

		private IReliableStateManager stateManager;

		public ModelAccessProvider(IReliableStateManager stateManager)
		{
			this.stateManager = stateManager;
		}
		/// <summary>
		/// Gets latest network model from Network Model Service and save in reliable dictionaries. If latest version was saved in reliable dictionaries, does nothing.
		/// </summary>
		/// <returns>Report about execution.</returns>
		public async Task<ExecutionReport> GetCurrentModel()
		{
			ExecutionReport executionReport = new ExecutionReport();

			GDAHelper gdaHelper = new GDAHelper();

			long currentNetworkModelVersion = await gdaHelper.GetVersion();

			if (currentNetworkModelVersion != -1)
			{
				var networkModelVersionDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(ReliableCollectionNames.NetworkModelVersionDictionary);

				long lastSavedVersion = -1;
				using (var tx = this.stateManager.CreateTransaction())
				{
					var result = await networkModelVersionDictionary.TryGetValueAsync(tx, ReliableCollectionNames.NetworkModelVersionDictionary);

					if (result.HasValue)
					{
						lastSavedVersion = result.Value;
					}
				}

				if (lastSavedVersion != -1)
				{
					if (lastSavedVersion == currentNetworkModelVersion)
					{
						string message = "Network model saved on model management service is up to date.";
						Logger.LogInformation(message);
						executionReport.Message = message;
						executionReport.Status = ExecutionStatus.SUCCESS;
					}
					else
					{
						Logger.LogDebug($"Network model saved on model management service ({lastSavedVersion}) is not same as version on network model service ({currentNetworkModelVersion}).");
						try
						{
							await InitializeNetworkModel(gdaHelper, currentNetworkModelVersion);
							string message = "Initialization of network model on model management service successfully completed.";
							executionReport.Message = message;
							executionReport.Status = ExecutionStatus.SUCCESS;
						}
						catch (Exception e)
						{
							string message = $"Initialize network model failed with error ${e.Message}";
							Logger.LogError(message, e);
							executionReport.Message = message;
							executionReport.Status = ExecutionStatus.ERROR;
						}
					}
				}
				else
				{
					Logger.LogDebug("There is no network model saved on model management service.");
					try
					{
						await InitializeNetworkModel(gdaHelper, currentNetworkModelVersion);
						string message = "Initialization of network model on model management service successfully completed.";
						executionReport.Message = message;
						executionReport.Status = ExecutionStatus.SUCCESS;
					}
					catch (Exception e)
					{
						string message = $"Initialize network model failed with error ${e.Message}";
						Logger.LogError(message, e);
						executionReport.Message = message;
						executionReport.Status = ExecutionStatus.ERROR;
					}
				}
			}
			else
			{
				string message = "Network model has not been initialized on network model service.";
				Logger.LogInformation(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.SUCCESS;
			}

			return executionReport;
		}

		/// <summary>
		/// Initialize topology by calling AnalyzeTopology from Topology Analyzer Service, with network model from reliable dictionary and save that topology in reliable dictionary.
		/// </summary>
		/// <returns>Report about execution</returns>
		public async Task<ExecutionReport> InitializeTopology()
		{
			ExecutionReport executionReport = new ExecutionReport();

			var networkModelVersionDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(ReliableCollectionNames.NetworkModelVersionDictionary);

			long lastSavedVersion = -1;
			using (var tx = this.stateManager.CreateTransaction())
			{
				var result = await networkModelVersionDictionary.TryGetValueAsync(tx, ReliableCollectionNames.NetworkModelVersionDictionary);

				if (result.HasValue)
				{
					lastSavedVersion = result.Value;
				}
			}

			if (lastSavedVersion == -1)
			{
				string message = "There is no network model saved on model management service.";
				Logger.LogDebug(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.SUCCESS;
				return executionReport;
			}

			InternalModelBuilderCIM internalModelBuilder = new InternalModelBuilderCIM(new CModelFramework());

			try
			{
				await InitializeInternalTopologyModel(internalModelBuilder);
			}
			catch (Exception e)
			{
				string message = $"Initialization of internal model failed with error: {e.Message}.";
				Logger.LogError(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.ERROR;
				return executionReport;
			}

			TopologyResult topologyResult = null;
			try
			{
				topologyResult = await TopologyAnalyzer.AnalyzeTopology(internalModelBuilder.InternalModel);
			}
			catch(Exception e)
			{
				string message = $"Analyze topology failed with error {e.Message}";
				Logger.LogError(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.ERROR;
				return executionReport;
			}

			if (topologyResult != null)
			{
				try
				{
					await SaveTopologyModel(topologyResult);
					string message = $"Save topology successfully completed";
					Logger.LogInformation(message);
					executionReport.Message = message;
					executionReport.Status = ExecutionStatus.SUCCESS;
				}
				catch (Exception e)
				{
					string message = $"Save topology model failed with error: {e.Message}";
					Logger.LogError(message, e);
					executionReport.Message = message;
					executionReport.Status = ExecutionStatus.ERROR;
				}
			}
			else
			{
				string message = $"Some error ocurred on topology analyzer service";
				Logger.LogError(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.ERROR;
			}


			return executionReport;
		}

		public async Task<ExecutionReport> GetOpenDSSScript()
		{
			ExecutionReport executionReport = new ExecutionReport();

			var networkModelVersionDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(ReliableCollectionNames.NetworkModelVersionDictionary);

			long lastSavedVersion = -1;
			using (var tx = this.stateManager.CreateTransaction())
			{
				var result = await networkModelVersionDictionary.TryGetValueAsync(tx, ReliableCollectionNames.NetworkModelVersionDictionary);

				if (result.HasValue)
				{
					lastSavedVersion = result.Value;
				}
			}

			if (lastSavedVersion == -1)
			{
				string message = "There is no network model saved on model management service.";
				Logger.LogDebug(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.SUCCESS;
				return executionReport;
			}

			var topologyResultDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(ReliableCollectionNames.TopologyResultDictionary);

			TopologyResult topologyResult = null;
			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				if (await topologyResultDictionary.GetCountAsync(tx) == 0)
				{
					string message = "There is no topology model saved on model management service.";
					Logger.LogDebug(message);
					executionReport.Message = message;
					executionReport.Status = ExecutionStatus.SUCCESS;
					return executionReport;
				}

				var enumerator = (await topologyResultDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

				await enumerator.MoveNextAsync(new CancellationToken());

				topologyResult = enumerator.Current.Value;
				
			}

			Dictionary<long, ResourceDescription> energySources = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.SourcesDictionary);
			Dictionary<long, ResourceDescription> connectivityNodes = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.NodeDictionary);
			Dictionary<long, ResourceDescription> branches = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.BranchDictionary);
			Dictionary<long, ResourceDescription> switches = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.SwitchDictionary);
			Dictionary<long, ResourceDescription> terminals = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.TerminalDictioary);
			Dictionary<long, ResourceDescription> otherElemens = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.OtherElementDictionary);

			List<Dictionary<long, ResourceDescription>> dictionaryList = new List<Dictionary<long, ResourceDescription>>() { energySources, connectivityNodes, branches, switches, terminals, otherElemens };

			Dictionary<long, ResourceDescription> networkModel = dictionaryList.SelectMany(dict => dict)
																			   .ToDictionary(pair => pair.Key, pair => pair.Value);


			OpenDSSScriptBuilder openDSSScriptBuilder = new OpenDSSScriptBuilder(networkModel, topologyResult);

			try
			{
				executionReport.Message = openDSSScriptBuilder.GenerateDSSScript();
				executionReport.Status = ExecutionStatus.SUCCESS;
			} 
			catch (Exception e)
			{
				string message = $"Generate DSS Script failed with error: {e.Message}";
				Logger.LogError(message, e);
				executionReport.Message = message;
			}

			return executionReport;
		}

		#region Private methods
		private async Task InitializeNetworkModel(GDAHelper gdaHelper, long modelVersion)
		{
			//Getting elements from NMS
			ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
			Dictionary<long, ResourceDescription> cimTerminals = (await gdaHelper.GetExtentValues(ModelCode.TERMINAL)).ToDictionary(value => value.Id);
			Dictionary<long, ResourceDescription> cimConnectivityNodes = (await gdaHelper.GetExtentValues(ModelCode.CONNECTIVITYNODE)).ToDictionary(value => value.Id);
			Dictionary<long, ResourceDescription> cimSwitches = (await gdaHelper.GetExtentValues(ModelCode.SWITCH)).ToDictionary(value => value.Id);
			Dictionary<long, ResourceDescription> cimEnergySources = (await gdaHelper.GetExtentValues(ModelCode.ENERGYSOURCE)).ToDictionary(value => value.Id);

			List<ModelCode> classicBranchElements = resourcesDesc.ClassicBranches;
			Dictionary<long, ResourceDescription> cimBranches = new Dictionary<long, ResourceDescription>();
			foreach (ModelCode classicBranchElement in classicBranchElements)
			{
				List<ResourceDescription> branches = await gdaHelper.GetExtentValues(classicBranchElement);

				foreach (ResourceDescription branch in branches)
				{
					cimBranches.Add(branch.Id, branch);
				}
			}

			Dictionary<long, ResourceDescription> otherCimElements = new Dictionary<long, ResourceDescription>();
			foreach (ModelCode otherElement in resourcesDesc.OtherElements)
			{
				List<ResourceDescription> elements = await gdaHelper.GetExtentValues(otherElement);
				foreach (ResourceDescription element in elements)
				{
					otherCimElements.Add(element.Id, element);
				}
			}

			//Clear dictionaries
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.TerminalDictioary);
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.NodeDictionary);
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.SwitchDictionary);
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.BranchDictionary);
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.SourcesDictionary);
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.OtherElementDictionary);
			await ClearDictionary<string, long>(ReliableCollectionNames.NetworkModelVersionDictionary);

			//Populate Dictionaries
			await InsertInReliableDictionary(ReliableCollectionNames.TerminalDictioary, cimTerminals);
			await InsertInReliableDictionary(ReliableCollectionNames.NodeDictionary, cimConnectivityNodes);
			await InsertInReliableDictionary(ReliableCollectionNames.SwitchDictionary, cimSwitches);
			await InsertInReliableDictionary(ReliableCollectionNames.BranchDictionary, cimBranches);
			await InsertInReliableDictionary(ReliableCollectionNames.SourcesDictionary, cimEnergySources);
			await InsertInReliableDictionary(ReliableCollectionNames.OtherElementDictionary, otherCimElements);


			//Save current version
			var networkModelVersionDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(ReliableCollectionNames.NetworkModelVersionDictionary);

			using (var tx = this.stateManager.CreateTransaction())
			{
				await networkModelVersionDictionary.AddAsync(tx, ReliableCollectionNames.NetworkModelVersionDictionary, modelVersion);
				await tx.CommitAsync();
			}
		}

		private async Task ClearDictionary<T1, T2>(string dictionaryName) where T1 : IComparable<T1>, IEquatable<T1>
		{
			var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<T1, T2>>(dictionaryName);
			await reliableDictionary.ClearAsync();
		}

		private async Task InsertInReliableDictionary(string dictionaryName, Dictionary<long, ResourceDescription> dictionary)
		{
			var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, ResourceDescription>>(dictionaryName);
			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				foreach (KeyValuePair<long, ResourceDescription> keyValuePair in dictionary)
				{
					bool success = await reliableDictionary.TryAddAsync(tx, keyValuePair.Key, keyValuePair.Value);
					if (!success)
					{
						Logger.LogWarning($"Element with gid {keyValuePair.Key} is not added in dictionary.");
					}
				}

				await tx.CommitAsync();
			}
		}

		private async Task InitializeInternalTopologyModel(InternalModelBuilderCIM internalModelBuilder)
		{
			Dictionary<long, ResourceDescription> energySources = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.SourcesDictionary);
			Dictionary<long, ResourceDescription> connectivityNodes = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.NodeDictionary);
			Dictionary<long, ResourceDescription> branches = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.BranchDictionary);
			Dictionary<long, ResourceDescription> switches = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.SwitchDictionary);
			Dictionary<long, ResourceDescription> terminals = await GetAllElementsFromReliableDictionary(ReliableCollectionNames.TerminalDictioary);

			internalModelBuilder.ReadSources(energySources, terminals);
			internalModelBuilder.ReadConnectivityNodes(connectivityNodes, terminals);
			internalModelBuilder.ReadBranches(branches, terminals);
			internalModelBuilder.ReadSwitches(switches, terminals);

		}

		private async Task<Dictionary<long, ResourceDescription>> GetAllElementsFromReliableDictionary(string dictionaryName)
		{
			Dictionary<long, ResourceDescription> returnValue = new Dictionary<long, ResourceDescription>();
			var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, ResourceDescription>>(dictionaryName);

			using (var tx = this.stateManager.CreateTransaction())
			{
				var enumerator = (await reliableDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
				try
				{
					while (await enumerator.MoveNextAsync(new CancellationToken()))
					{
						returnValue.Add(enumerator.Current.Key, enumerator.Current.Value);
					}
				}
				catch (Exception e)
				{
					throw e;
				}
			}

			return returnValue;
		}

		private async Task SaveTopologyModel(TopologyResult topologyResult)
		{
			var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(ReliableCollectionNames.TopologyResultDictionary);
			await reliableDictionary.ClearAsync();

			//var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(topologyResultsDictionaryName);

			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				//TODO: Prosledi nodeid, to ce ici za vise root-ova
				await reliableDictionary.AddAsync(tx, topologyResult.Nodes[0].Lid, topologyResult);
				await tx.CommitAsync();
			}
		}

		

		#endregion
	}
}
