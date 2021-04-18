using Common;
using Common.Clients;
using Common.LoadFlowInterfaces;
using Common.Logger;
using Common.ModelManagerInterfaces;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using OpenDSSengine;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace LoadFlowSolver
{
	/// <summary>
	/// An instance of this class is created for each service replica by the Service Fabric runtime.
	/// </summary>
	internal sealed class LoadFlowSolver : StatefulService, ILoadFlowSolver
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		private IModelAccessContract ModelAccessContract { get { return ModelAccessClient.CreateClient(); } }
		public LoadFlowSolver(StatefulServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);
		}

		public async Task SolveLoadFlow(long rootId)
		{
			Logger.LogInformation($"LoadFlowSolver.SolveLoadFlow started. Partition id = {Context.PartitionId}");

			ExecutionReport executionReport = await ModelAccessContract.GetOpenDSSScript(rootId);

			if (executionReport.Status == ExecutionStatus.ERROR)
			{
				Logger.LogError($"Load flow error failed with error: {executionReport.Message}");
			}

			Logger.LogInformation($"{executionReport.Message}");

			DSS DSSObject = new DSS();
			
			if (!(DSSObject.Start(0)))
			{
				Logger.LogError("DSS failed to start.");
				return;
			}

			DSSObject.Text.Command = "Clear";

			List<string> commands = executionReport.Message.Split('\n').ToList();

			foreach (string command in commands)
			{
				DSSObject.Text.Command = command.Replace("\r", "");
			}
			
			DSSObject.ActiveCircuit.Solution.Solve();

			if (DSSObject.ActiveCircuit.Solution.Converged)
			{
				Logger.LogInformation("Solution converged");
			}
			else
			{
				Logger.LogInformation("Solution did not converge.");
			}

		}

		/// <summary>
		/// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
		/// </summary>
		/// <remarks>
		/// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
		/// </remarks>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			return new List<ServiceReplicaListener>
			{
				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<ILoadFlowSolver>(context,
																		 this,
																		 WcfUtility.CreateTcpListenerBinding(),
																		 "LoadFlowSolverEndpoint");
				}, "LoadFlowSolverEndpoint")
			};
		}

		/// <summary>
		/// This is the main entry point for your service replica.
		/// This method executes when this replica of your service becomes primary and has write status.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			// TODO: Replace the following sample code with your own logic 
			//       or remove this RunAsync override if it's not needed in your service.

			//var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

			//while (true)
			//{
			//	cancellationToken.ThrowIfCancellationRequested();

			//	using (var tx = this.StateManager.CreateTransaction())
			//	{
			//		var result = await myDictionary.TryGetValueAsync(tx, "Counter");

			//		ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
			//			result.HasValue ? result.Value.ToString() : "Value does not exist.");

			//		await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

			//		// If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
			//		// discarded, and nothing is saved to the secondary replicas.
			//		await tx.CommitAsync();
			//	}

			//	await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
			//}
		}
	}
}
