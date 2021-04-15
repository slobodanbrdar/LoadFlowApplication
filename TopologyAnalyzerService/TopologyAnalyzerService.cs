using Common.Logger;
using Common.TopologyServiceInterfaces;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.TopologyService;
using Common.TopologyService.InternalModel.Circuts;
using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using Common.TopologyService.InternalModelBuilder;
using Common.TopologyService.MatrixModel;
using Common.TopologyService.TopologyAnalyzer;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TopologyAnalyzerService
{
	/// <summary>
	/// An instance of this class is created for each service instance by the Service Fabric runtime.
	/// </summary>
	internal sealed class TopologyAnalyzerService : StatelessService, ITopologyAnalyzer
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public TopologyAnalyzerService(StatelessServiceContext context)
			: base(context)
		{
			this.logger = CloudLoggerFactory.GetLogger(ServiceEventSource.Current, context);
		}

		public Task<TopologyResult> AnalyzeTopology(CModelFramework internalModel)
		{
			ServiceEventSource.Current.ServiceMessage(this.Context, "TopologyStatelessService.Analyze started. PartitionID = {0}.", Context.PartitionId);

			// Create matrix model
			CSparseMatrix matrixModel = new CSparseMatrix();
			CMatrixModelBuilder matrixModelBuilder = new CMatrixModelBuilder(matrixModel, internalModel);
			matrixModelBuilder.CreateInitialLidDictionary();
			matrixModelBuilder.CreateInitialMatrixModel();
			matrixModelBuilder.InsertSwitchDeviceFromInternalToMatrixModel();

			CTopologyAnalyzer topologyAnalyzer = new CTopologyAnalyzer(matrixModel, matrixModelBuilder.LIDtoIND, matrixModelBuilder.INDtoLID, internalModel);


			foreach (MPRoot root in internalModel.Roots.Values)
			{
				if (!root.UpToDate)     //obradjuju se samo neazurni koreni
				{
					topologyAnalyzer.UpdateRootTopology(root);
				}
			}


			List<MPNode> nodes = topologyAnalyzer.InternalModel.Nodes.Values.ToList();
			List<MPBranch> branches = topologyAnalyzer.InternalModel.Branches.Values.ToList();

			ServiceEventSource.Current.ServiceMessage(this.Context, "TopologyStatelessService.Analyze finished.");

			return Task.FromResult(new TopologyResult(nodes, branches));
		}

		/// <summary>
		/// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
		/// </summary>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
		{
			return new List<ServiceInstanceListener>
			{
				new ServiceInstanceListener (context =>
				{
					return new WcfCommunicationListener<ITopologyAnalyzer>(context,
																		   this,
																		   WcfUtility.CreateTcpListenerBinding(),
																		   endpointResourceName: "TopologyAnalyzerServiceEndpoint");
				}, "TopologyAnalyzerServiceEndpoint")
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
