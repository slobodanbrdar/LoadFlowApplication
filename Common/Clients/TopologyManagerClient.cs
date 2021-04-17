using Common.TopologyService;
using Common.TopologyService.InternalModelBuilder;
using Common.TopologyServiceInterfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class TopologyManagerClient : WcfSeviceFabricClientBase<ITopologyRequest>, ITopologyRequest
	{
		private static readonly string listenerName = "TopologyManagerServiceEndpoint";
		private static readonly string serviceUri = "fabric:/LoadFlowApplication/TopologyManagerService";

		public TopologyManagerClient(WcfCommunicationClientFactory<ITopologyRequest> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base (clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ITopologyRequest CreateClient()
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<TopologyManagerClient, ITopologyRequest>(uri, ServiceType.STATELESS_SERVICE);
		}

		public async Task<IEnumerable<TopologyResult>> AnalyzeTopology(CModelFramework internalModel)
		{
			return await InvokeWithRetryAsync(client => client.Channel.AnalyzeTopology(internalModel));
		}
	}
}
