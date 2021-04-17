using Common.LoadFlowInterfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OpenDSSengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class LoadFlowSolverClient : WcfSeviceFabricClientBase<ILoadFlowSolver>, ILoadFlowSolver
	{
		private static string listenerName = "LoadFlowSolverEndpoint";
		private static string serviceUri = "fabric:/LoadFlowApplication/LoadFlowSolver";

		public LoadFlowSolverClient(WcfCommunicationClientFactory<ILoadFlowSolver> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			:base (clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ILoadFlowSolver CreateClient(int partitionKey = 0)
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<LoadFlowSolverClient, ILoadFlowSolver>(uri, ServiceType.STATEFUL_SERVICE);
		}

		public async Task SolveLoadFlow()
		{
			await InvokeWithRetryAsync(client => client.Channel.SolveLoadFlow());
		}
	}
}
