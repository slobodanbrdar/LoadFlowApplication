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

		private ITopologyRequest TopologyRequest
		{
			get
			{
				return TopologyManagerClient.CreateClient();
			}
		}

		private IReliableStateManager stateManager;

		public ModelAccessProvider(IReliableStateManager stateManager)
		{
			this.stateManager = stateManager;
		}
		

		public async Task<ExecutionReport> InitializeTopology()
		{
			ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
			ExecutionReport executionReport = new ExecutionReport();
			await ClearDictionary<long, ResourceDescription>(ReliableCollectionNames.NetworkModelDictinoary);

			GDAHelper gdaHelper = new GDAHelper();

			Dictionary<long, ResourceDescription> energySources = (await gdaHelper.GetExtentValues(ModelCode.ENERGYSOURCE)).ToDictionary(value => value.Id);
			Dictionary<long, ResourceDescription> connectivityNodes = (await gdaHelper.GetExtentValues(ModelCode.CONNECTIVITYNODE)).ToDictionary(value => value.Id);
			Dictionary<long, ResourceDescription> switches = (await gdaHelper.GetExtentValues(ModelCode.SWITCH)).ToDictionary(value => value.Id);
			Dictionary<long, ResourceDescription> terminals = (await gdaHelper.GetExtentValues(ModelCode.TERMINAL)).ToDictionary(value => value.Id);


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

			List<Task> tasks = new List<Task>()
			{
				SaveNetworkModelElements(energySources),
				SaveNetworkModelElements(connectivityNodes),
				SaveNetworkModelElements(switches),
				SaveNetworkModelElements(terminals),
				SaveNetworkModelElements(cimBranches),
				SaveNetworkModelElements(otherCimElements),
			};

			InternalModelBuilderCIM internalModelBuilder = new InternalModelBuilderCIM(new CModelFramework());

			internalModelBuilder.ReadSources(energySources, terminals);
			internalModelBuilder.ReadConnectivityNodes(connectivityNodes, terminals);
			internalModelBuilder.ReadBranches(cimBranches, terminals);
			internalModelBuilder.ReadSwitches(switches, terminals);

			IEnumerable<TopologyResult> topologyResults = null;
			try
			{
				topologyResults = await TopologyRequest.AnalyzeTopology(internalModelBuilder.InternalModel);
			}
			catch (Exception e)
			{
				string message = $"Analyze topology failed with error {e.Message}";
				Logger.LogError(message);
				executionReport.Message = message;
				executionReport.Status = ExecutionStatus.ERROR;
				return executionReport;
			}

			if (topologyResults != null)
			{
				try
				{
					await SaveTopologyModel(topologyResults);
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


			await Task.WhenAll(tasks);

			//Save network model version
			long currentNetworkModelVersion = await gdaHelper.GetVersion();

			var networkModelVersionDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, long>>(ReliableCollectionNames.NetworkModelVersionDictionary);
			await networkModelVersionDictionary.ClearAsync();
			using (var tx = this.stateManager.CreateTransaction())
			{
				await networkModelVersionDictionary.AddAsync(tx, ReliableCollectionNames.NetworkModelVersionDictionary, currentNetworkModelVersion);
				await tx.CommitAsync();
			}

			return executionReport;
		}

		public async Task<ExecutionReport> GetOpenDSSScript(long rootId)
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

				var result = await topologyResultDictionary.TryGetValueAsync(tx, rootId);

				if (!result.HasValue)
				{
					string message = $"Root id {rootId} is not presented in topology model";
					Logger.LogError(message);
					executionReport.Message = message;
					executionReport.Status = ExecutionStatus.ERROR;
					return executionReport;
				}

				topologyResult = result.Value;
				
			}

			Dictionary<long, ResourceDescription> networkModel = await GetAllNetworkModelElementsFromReliableDictionary(ReliableCollectionNames.NetworkModelDictinoary);

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

		public async Task<IEnumerable<long>> GetRootIDs()
		{
			List<long> rootIds = new List<long>();

			var topologyResultDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(ReliableCollectionNames.TopologyResultDictionary);

			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				var enumerator = (await topologyResultDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

				while (await enumerator.MoveNextAsync(new CancellationToken()))
				{
					rootIds.Add(enumerator.Current.Key);
				}
			}

			return rootIds;
		}

		#region Private methods

		private async Task ClearDictionary<T1, T2>(string dictionaryName) where T1 : IComparable<T1>, IEquatable<T1>
		{
			var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<T1, T2>>(dictionaryName);
			await reliableDictionary.ClearAsync();
		}

		private async Task<Dictionary<long, ResourceDescription>> GetAllNetworkModelElementsFromReliableDictionary(string dictionaryName)
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

		private async Task SaveTopologyModel(IEnumerable<TopologyResult> topologyResults)
		{
			var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(ReliableCollectionNames.TopologyResultDictionary);
			await reliableDictionary.ClearAsync();

			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				List<Task> tasks = new List<Task>();
				foreach (TopologyResult topologyResult in topologyResults)
				{
					tasks.Add(reliableDictionary.AddOrUpdateAsync(tx, topologyResult.RootId, topologyResult, (key, value) => topologyResult));
				}

				await Task.WhenAll(tasks);
				await tx.CommitAsync();
			}
		}

		private async Task SaveNetworkModelElements(Dictionary<long, ResourceDescription> elements)
		{
			var networkModelDictinary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, ResourceDescription>>(ReliableCollectionNames.NetworkModelDictinoary);

			using (var tx = this.stateManager.CreateTransaction())
			{
				List<Task> tasks = new List<Task>();
				foreach (var keyValuePair in elements)
				{
					tasks.Add(networkModelDictinary.AddOrUpdateAsync(tx, keyValuePair.Key, keyValuePair.Value, (key, value) => keyValuePair.Value));
				}

				await Task.WhenAll(tasks);
				await tx.CommitAsync();
			}
		}

		

		#endregion
	}
}
