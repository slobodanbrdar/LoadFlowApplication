using Common.Clients;
using Common.LoadFlowInterfaces;
using Common.Logger;
using Common.ModelManagerInterfaces;
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

namespace LoadFlowManagerService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class LoadFlowManagerService : StatelessService, ILoadFlowManagerContract
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private IModelAccessContract ModelAccessContract
		{
			get
			{
				return ModelAccessClient.CreateClient();
			}
		}


		public LoadFlowManagerService(StatelessServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);
		}

		public async Task StartLoadFlowSolving()
		{

			List<long> rootIds = (await ModelAccessContract.GetRootIDs()).ToList();

			Logger.LogInformation($"Number of roots is: {rootIds.Count}");

			FabricClient fabricClient = new FabricClient();
			int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/LoadFlowApplication/LoadFlowSolver"))).Count;

			Logger.LogInformation($"Number of partitions is {partitionsNumber}");

			int index = 0;
			List<Task> loadFlowTasks = new List<Task>();
			foreach(long rootId in rootIds)
			{
				ILoadFlowSolver loadFlowSolver = LoadFlowSolverClient.CreateClient(index % partitionsNumber);

				loadFlowTasks.Add(loadFlowSolver.SolveLoadFlow(rootId));

				index++;
				
			}

			await Task.WhenAll(loadFlowTasks);

			Logger.LogInformation("Solving of load flow successfully finished.");

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
					return new WcfCommunicationListener<ILoadFlowManagerContract>(context,
																			  this,
																			  WcfUtility.CreateTcpListenerBinding(),
																			  "LoadFlowManagerEndpoint");
				}, "LoadFlowManagerEndpoint"),
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
