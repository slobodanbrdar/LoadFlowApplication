using Common.ModelManagerInterfaces;
using Common.TopologyService;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class TopologyModelAccessClient : WcfSeviceFabricClientBase<ITopologyModelAccessContract>, ITopologyModelAccessContract
	{
		private static readonly string listenerName = "TopologyModelAccessEndpoint";
		private static readonly string serviceUri = "fabric:/LoadFlowApplication/ModelManagerService";
		public TopologyModelAccessClient(WcfCommunicationClientFactory<ITopologyModelAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base (clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ITopologyModelAccessContract CreateClient()
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<TopologyModelAccessClient, ITopologyModelAccessContract>(uri, ServiceType.STATEFUL_SERVICE);
		}

		public async Task<TopologyResult> GetTopologyResult()
		{
			return await InvokeWithRetryAsync(client => client.Channel.GetTopologyResult());
		}
	}
}
