using Common;
using Common.Clients;
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

namespace ModelManagerImplementation.LoadFlowModelAccess
{
	public class TopologyModelAccess : ITopologyModelAccessContract
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

		private Dictionary<long, ResourceDescription> cimTerminals;
		private Dictionary<long, ResourceDescription> cimConnectivityNodes;
		private Dictionary<long, ResourceDescription> cimSwitches;
		private Dictionary<long, ResourceDescription> cimBranches;
		private Dictionary<long, ResourceDescription> cimEnergySources;
		private Dictionary<long, ResourceDescription> otherCimElements;
		private InternalModelBuilderCIM internalModelBuilder;
		private GDAHelper gdaHelper;
		private long lastCimVersion = -1;

		private IReliableStateManager stateManager;


		public TopologyModelAccess(IReliableStateManager stateManager)
		{
			internalModelBuilder = new InternalModelBuilderCIM(new CModelFramework());
			gdaHelper = new GDAHelper();
			this.stateManager = stateManager;
		}
		public async Task<TopologyResult> GetTopologyResult()
		{
			TopologyResult topologyResult = null;

			var topologyModelReliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(ReliableCollectionNames.TopologyResultDictionary);

			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				var enumerator = (await topologyModelReliableDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

				await enumerator.MoveNextAsync(new CancellationToken());

				topologyResult = enumerator.Current.Value;
			}

			return topologyResult;
		}

		//public async Task<TopologyResult> GetTopologyResult()
		//{
		//	Logger.LogDebug($"GetTopologyResult started.");
		//	var topologyResultCV = await TryGetTopologyAsyncFromDictionary();

		//	if (topologyResultCV.HasValue)
		//	{
		//		Logger.LogInformation($"Topology result successfully retrived from reliable dictionary.");
		//		return topologyResultCV.Value;
		//	}

		//	ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();

		//	await GetNetworkModel(modelResourcesDesc);
		//	Logger.LogDebug("GetNetworkModel successfully finished.");

		//	try
		//	{
		//		await SaveNetworkModel();
		//		Logger.LogDebug("Network model successfully saved in reliable dictionaries.");
		//	}
		//	catch (Exception e)
		//	{
		//		Logger.LogError($"Saving network model failed with error: {e.Message}.");
		//	}

		//	InitializeInternalModel(); 
		//	Logger.LogDebug("InitializeInternalModel successfully finished.");

		//	TopologyResult topologyResult = await GetTopology();
		//	try
		//	{
		//		await SaveTopologyModel(topologyResult);
		//		Logger.LogDebug("Topology model successfully saved in reliable collections.");
		//	}
		//	catch (Exception e)
		//	{
		//		Logger.LogError($"Saving topology model failed with error: {e.Message}.", e);
		//	}

		//	await SaveLastVersion();
		//	Logger.LogDebug($"Last version ({lastCimVersion}) successfully saved.");

		//	return topologyResult;
		//}

		//private async Task<ConditionalValue<TopologyResult>> TryGetTopologyAsyncFromDictionary()
		//{
		//	ConditionalValue<TopologyResult> conditionalValue = new ConditionalValue<TopologyResult>(false, null);

		//	long currentVersion = await gdaHelper.GetVersion();

		//	if(currentVersion == lastCimVersion && currentVersion != -1)
		//	{
		//		var lastVersionDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, List<long>>>(lastCimModelVersionName);
		//		List<long> rootIds = new List<long>();
		//		using (ITransaction tx = this.stateManager.CreateTransaction())
		//		{
		//			var result = await lastVersionDictionary.TryGetValueAsync(tx, currentVersion);
		//			if (result.HasValue)
		//			{
		//				rootIds = result.Value;
		//			}
		//			else
		//			{
		//				string errMsg = $"Last version dictionary doesn't have value for version {currentVersion}.";
		//				Logger.LogError(errMsg);
		//				throw new Exception(errMsg);
		//			}
		//		}
		//		var topologyModelDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(topologyResultsDictionaryName);
		//		using (ITransaction tx = this.stateManager.CreateTransaction())
		//		{
		//			var result = await topologyModelDictionary.TryGetValueAsync(tx, rootIds[0]);
		//			TopologyResult topologyResult = null;
		//			if (result.HasValue)
		//			{
		//				topologyResult = result.Value;
		//			}
		//			else
		//			{
		//				string errMsg = $"Topology result for id {rootIds[0]} is not in dictionary.";
		//				Logger.LogError(errMsg);
		//				throw new Exception(errMsg);
		//			}
		//			return new ConditionalValue<TopologyResult>(true, topologyResult);
		//		}
		//	}
		//	else
		//	{
		//		Logger.LogDebug("CIM version on network model service and model manager service is not same. Topology will be create...");
		//	}

		//	return conditionalValue;
		//}

		//private async Task GetNetworkModel(ModelResourcesDesc resourcesDesc)
		//{
		//	cimTerminals = (await gdaHelper.GetExtentValues(ModelCode.TERMINAL)).ToDictionary(value => value.Id);
		//	cimConnectivityNodes = (await gdaHelper.GetExtentValues(ModelCode.CONNECTIVITYNODE)).ToDictionary(value => value.Id);
		//	cimSwitches = (await gdaHelper.GetExtentValues(ModelCode.SWITCH)).ToDictionary(value => value.Id);
		//	cimEnergySources = (await gdaHelper.GetExtentValues(ModelCode.ENERGYSOURCE)).ToDictionary(value => value.Id);

		//	List<ModelCode> classicBranchElements = resourcesDesc.ClassicBranches;
		//	cimBranches = new Dictionary<long, ResourceDescription>();
		//	foreach (ModelCode classicBranchElement in classicBranchElements)
		//	{
		//		List<ResourceDescription> branches = await gdaHelper.GetExtentValues(classicBranchElement);

		//		foreach(ResourceDescription branch in branches)
		//		{
		//			cimBranches.Add(branch.Id, branch);
		//		}
		//	}

		//	otherCimElements = new Dictionary<long, ResourceDescription>();
		//	foreach (ModelCode otherElement in resourcesDesc.OtherElements)
		//	{
		//		List<ResourceDescription> elements = await gdaHelper.GetExtentValues(otherElement);
		//		foreach(ResourceDescription element in elements)
		//		{
		//			otherCimElements.Add(element.Id, element);
		//		}
		//	}

		//	lastCimVersion = await gdaHelper.GetVersion();
		//}

		//private async Task SaveNetworkModel()
		//{
		//	await ClearDictionaries();
		//	await InsertInReliableDictionary(terminalsDictionaryName, cimTerminals);
		//	await InsertInReliableDictionary(nodesDictionartName, cimConnectivityNodes);
		//	await InsertInReliableDictionary(switchesDictionaryName, cimSwitches);
		//	await InsertInReliableDictionary(branchesDictionaryName, cimBranches);
		//	await InsertInReliableDictionary(sourcesDictionaryName, cimEnergySources);
		//	await InsertInReliableDictionary(otherElementsDictionaryName, otherCimElements);

		//}

		//private async Task ClearDictionaries()
		//{
		//	await ClearDictionary(terminalsDictionaryName);
		//	await ClearDictionary(nodesDictionartName);
		//	await ClearDictionary(switchesDictionaryName);
		//	await ClearDictionary(branchesDictionaryName);
		//	await ClearDictionary(sourcesDictionaryName);
		//	await ClearDictionary(otherElementsDictionaryName);
		//}

		//private async Task InsertInReliableDictionary(string dictionaryName, Dictionary<long, ResourceDescription> dictionary)
		//{
		//	var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, ResourceDescription>>(dictionaryName);
		//	using (ITransaction tx = this.stateManager.CreateTransaction())
		//	{
		//		foreach (KeyValuePair<long, ResourceDescription> keyValuePair in dictionary)
		//		{
		//			bool success = await reliableDictionary.TryAddAsync(tx, keyValuePair.Key, keyValuePair.Value);
		//			if (!success)
		//			{
		//				Logger.LogWarning($"Element with gid {keyValuePair.Key} is not added in dictionary.");
		//			}
		//		}

		//		await tx.CommitAsync();
		//	}
		//}

		//private async Task ClearDictionary(string dictionaryName)
		//{
		//	var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, ResourceDescription>>(dictionaryName);
		//	await reliableDictionary.ClearAsync();
		//}

		//private void InitializeInternalModel()
		//{
		//	internalModelBuilder.ReadSources(cimEnergySources, cimTerminals);
		//	internalModelBuilder.ReadConnectivityNodes(cimConnectivityNodes, cimTerminals);
		//	internalModelBuilder.ReadBranches(cimBranches, cimTerminals);
		//	internalModelBuilder.ReadSwitches(cimSwitches, cimTerminals);

		//}

		//private async Task<TopologyResult> GetTopology()
		//{
		//	TopologyResult result = null;
		//	try
		//	{
		//		result = await TopologyAnalyzer.AnalyzeTopology(internalModelBuilder.InternalModel);
		//	}
		//	catch (Exception e)
		//	{
		//		Logger.LogError($"Get topology method failed with exception: {e.Message}", e);
		//	}

		//	return result;
		//}

		//private async Task SaveTopologyModel(TopologyResult topologyResult)
		//{
		//	var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(topologyResultsDictionaryName);
		//	await reliableDictionary.ClearAsync();

		//	//var reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, TopologyResult>>(topologyResultsDictionaryName);

		//	using (ITransaction tx = this.stateManager.CreateTransaction())
		//	{
		//		await reliableDictionary.AddAsync(tx, topologyResult.Nodes[0].Lid, topologyResult);
		//		await tx.CommitAsync();
		//	}
		//}

		//private async Task SaveLastVersion()
		//{
		//	var lastVersionCollection = await this.stateManager.GetOrAddAsync<IReliableDictionary<long, List<long>>>(lastCimModelVersionName);
		//	using (ITransaction tx = this.stateManager.CreateTransaction())
		//	{
		//		var previousVersion = await lastVersionCollection.TryGetValueAsync(tx, lastCimVersion);
		//		List<long> rootIds = cimEnergySources.Select(element => element.Key).ToList();

		//		if (previousVersion.HasValue)
		//		{
					
		//			bool success = await lastVersionCollection.TryUpdateAsync(tx, lastCimVersion, rootIds, previousVersion.Value);

		//			if (!success)
		//			{
		//				Logger.LogWarning($"Update of version {lastCimVersion} failed.");
		//			}
		//		}
		//		else
		//		{
		//			await lastVersionCollection.AddAsync(tx, lastCimVersion, rootIds);
		//		}

		//		await tx.CommitAsync();
		//	}
		//}

	}
}
