using Common.Clients;
using Common.TopologyService;
using Common.TopologyService.InternalModelBuilder;
using Common.TopologyServiceInterfaces;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TopologyManagerService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class TopologyManagerService : StatelessService, ITopologyRequest
	{
		public TopologyManagerService(StatelessServiceContext context)
			: base(context)
		{ }

		public async Task<IEnumerable<TopologyResult>> AnalyzeTopology(CModelFramework internalModel)
		{
			List<Task<TopologyResult>> topologyTasks = new List<Task<TopologyResult>>();

			int index = 0;

			//for (int i = 0; i < 50; i++)
			//{
			//	int partitionKey = GetPartitionKey(i);
			//	ITopologyAnalyzer topologyAnalyzer = TopologyAnalyzerClient.CreateClient(partitionKey);
			//	topologyTasks.Add(topologyAnalyzer.AnalyzeTopology(internalModel, internalModel.Roots.First().Key));
			//}

			foreach (var root in internalModel.Roots)
			{
				int partitionKey = GetPartitionKey(index);
				ITopologyAnalyzer topologyAnalyzer = TopologyAnalyzerClient.CreateClient(partitionKey);

				topologyTasks.Add(topologyAnalyzer.AnalyzeTopology(internalModel, root.Key));
				//topologyTasks[index].Start();

				index++;
			}

			List<TopologyResult> topologyResults = (await Task.WhenAll(topologyTasks)).ToList();

			return topologyResults;
		}

		private int GetPartitionKey(int index)
		{
			return index % 3;
		}

		/// <summary>
		/// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
		/// </summary>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
		{
			return new List<ServiceInstanceListener>
			{
				new ServiceInstanceListener(context =>
				{
					return new WcfCommunicationListener<ITopologyRequest>(context,
																		  this,
																		  WcfUtility.CreateTcpListenerBinding(),
																		  "TopologyManagerServiceEndpoint");
				}, "TopologyManagerServiceEndpoint"),
			};
		}

		/// <summary>
		/// This is the main entry point for your service instance.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			// TODO: Replace the following sample code with your own logic 
			//       or remove this RunAsync override if it's not needed in your service.

			//long iterations = 0;

			//while (true)
			//{
			//	cancellationToken.ThrowIfCancellationRequested();

			//	ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

			//	await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
			//}
		}
	}
}
