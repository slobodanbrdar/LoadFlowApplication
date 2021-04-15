using Common.GDAInterfaces;
using Common.Logger;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NMSGdaImplementation;
using NMSGdaImplementation.GDA;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NMSGdaService
{
	/// <summary>
	/// An instance of this class is created for each service replica by the Service Fabric runtime.
	/// </summary>
	internal sealed class NMSGdaService : StatefulService
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private readonly NetworkModel networkModel;
		private readonly INetworkModelGDAContract genericDataAccess;
		public NMSGdaService(StatefulServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);
			Logger.LogInformation("GdaService Constructor started.");
			this.networkModel = new NetworkModel(this.StateManager);
			this.genericDataAccess = new GenericDataAccess(networkModel);
			Logger.LogInformation("GdaService finished");
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
			return new List<ServiceReplicaListener>()
			{
				new ServiceReplicaListener(context =>
				{
					return new WcfCommunicationListener<INetworkModelGDAContract>(context,
																				  genericDataAccess,
																				  WcfUtility.CreateTcpListenerBinding(),
																				  "NMSGdaServiceEndpoint");

				}, "NMSGdaServiceEndpoint")
				//new ServiceReplicaListener(context =>
				//{
				//	return new WcfCommunicationListener<ITest>(context,
				//											   this,
				//											   WcfUtility.CreateTcpListenerBinding(),
				//											   "TestEndpoint");

				//})

			};
		}

		/// <summary>
		/// This is the main entry point for your service replica.
		/// This method executes when this replica of your service becomes primary and has write status.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
		protected override async Task RunAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				Logger.LogInformation("Network model initialization started.");
				await this.networkModel.Initialize();
				Logger.LogInformation("Network model initialization finished.");

			}
			catch (Exception e)
			{
				string message = $"Run async exception on networkModel.Initialize(). Message: {e.Message}";
				Logger.LogError(message, e);
			}
		}

		
	}
}
