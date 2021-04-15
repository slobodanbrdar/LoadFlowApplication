﻿using Common.TopologyService;
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
	public class TopologyAnalyzerClient : WcfSeviceFabricClientBase<ITopologyAnalyzer>, ITopologyAnalyzer
	{
		private static readonly string listenerName = "TopologyAnalyzerServiceEndpoint";
		private static readonly string serviceUri = "fabric:/LoadFlowApplication/TopologyAnalyzerService";

		public TopologyAnalyzerClient(WcfCommunicationClientFactory<ITopologyAnalyzer> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base (clientFactory, serviceUri, servicePartition, listenerName)
		{
				
		}

		public static ITopologyAnalyzer CreateClient()
		{
			Uri uri = new Uri(serviceUri);
			ClientFactory clientFactory = new ClientFactory();
			return clientFactory.CreateClient<TopologyAnalyzerClient, ITopologyAnalyzer>(uri, ServiceType.STATELESS_SERVICE);
		}

		public async Task<TopologyResult> AnalyzeTopology(CModelFramework internalModel)
		{
			return await InvokeWithRetryAsync(client => client.Channel.AnalyzeTopology(internalModel));
		}
	}
}