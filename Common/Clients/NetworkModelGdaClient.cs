using Common.GDAInterfaces;
using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class NetworkModelGdaClient : WcfSeviceFabricClientBase<INetworkModelGDAContract>, INetworkModelGDAContract
	{
		private static readonly string listenerName = "NMSGdaServiceEndpoint";
		private static readonly string serviceUri = "fabric:/LoadFlowApplication/NMSGdaService";
		public NetworkModelGdaClient(WcfCommunicationClientFactory<INetworkModelGDAContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static INetworkModelGDAContract CreateClient()
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<NetworkModelGdaClient, INetworkModelGDAContract>(uri, ServiceType.STATEFUL_SERVICE);
		}

		public async Task<UpdateResult> ApplyUpdate(Delta delta)
		{
			return await InvokeWithRetryAsync(client => client.Channel.ApplyUpdate(delta));
		}

		public async Task<int> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			return await InvokeWithRetryAsync(client => client.Channel.GetExtentValues(entityType, propIds));
		}

		public async Task<int> GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			return await InvokeWithRetryAsync(client => client.Channel.GetRelatedValues(source, propIds, association));
		}

		public async Task<ResourceDescription> GetValues(long resourceId, List<ModelCode> propIds)
		{
			return await InvokeWithRetryAsync(client => client.Channel.GetValues(resourceId, propIds));
		}

		public async Task<long> GetVersion()
		{
			return await InvokeWithRetryAsync(client => client.Channel.GetVersion());
		}

		public async Task<bool> IteratorClose(int id)
		{
			return await InvokeWithRetryAsync(client => client.Channel.IteratorClose(id));
		}

		public async Task<List<ResourceDescription>> IteratorNext(int n, int id)
		{
			return await InvokeWithRetryAsync(client => client.Channel.IteratorNext(n, id));
		}

		public async Task<int> IteratorResourcesLeft(int id)
		{
			return await InvokeWithRetryAsync(client => client.Channel.IteratorResourcesLeft(id));
		}

		public async Task<int> IteratorResourcesTotal(int id)
		{
			return await InvokeWithRetryAsync(client => client.Channel.IteratorResourcesTotal(id));
		}

		public async Task<bool> IteratorRewind(int id)
		{
			return await InvokeWithRetryAsync(client => client.Channel.IteratorRewind(id));
		}
	}
}
