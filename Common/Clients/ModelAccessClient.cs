using Common.ModelManagerInterfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class ModelAccessClient : WcfSeviceFabricClientBase<IModelAccessContract>, IModelAccessContract
	{
		private static string listenerName = "ModelAccessEndpoint";
		private static string serviceUri = "fabric:/LoadFlowApplication/ModelManagerService";

		public ModelAccessClient(WcfCommunicationClientFactory<IModelAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base (clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static IModelAccessContract CreateClient(int partitionKey = 0)
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<ModelAccessClient, IModelAccessContract>(uri, ServiceType.STATEFUL_SERVICE);
		}

		public async Task<ExecutionReport> GetOpenDSSScript()
		{
			return await InvokeWithRetryAsync(client => client.Channel.GetOpenDSSScript());
		}

		public async Task<ExecutionReport> InitializeTopology()
		{
			return await InvokeWithRetryAsync(client => client.Channel.InitializeTopology());
		}
	}
}
