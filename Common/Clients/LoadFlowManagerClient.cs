using Common.LoadFlowInterfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class LoadFlowManagerClient : WcfSeviceFabricClientBase<ILoadFlowManagerContract>, ILoadFlowManagerContract
	{
		private static string listenerName = "LoadFlowManagerEndpoint";
		private static string serviceUri = "fabric:/LoadFlowApplication/LoadFlowManagerService";

		public LoadFlowManagerClient(WcfCommunicationClientFactory<ILoadFlowManagerContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ILoadFlowManagerContract CreateClient()
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<LoadFlowManagerClient, ILoadFlowManagerContract>(uri, ServiceType.STATELESS_SERVICE);
		}

		public async Task StartLoadFlowSolving()
		{
			await InvokeWithRetryAsync(client => client.Channel.StartLoadFlowSolving());
		}
	}
}
