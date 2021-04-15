using Common.Logger;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	internal class ClientFactory
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public ClientFactory()
		{

		}

		public TContract CreateClient<TClient, TContract>(Uri serviceUri, ServiceType serviceType, string serviceKey = null) where TContract : class, IService
																												where TClient : WcfSeviceFabricClientBase<TContract>
		{
			//TODO: ako bude trebalo service key resiti
			var binding = WcfUtility.CreateTcpClientBinding();
			var partitionResolver = ServicePartitionResolver.GetDefault();
			var wcfClientFactory = new WcfCommunicationClientFactory<TContract>(clientBinding: binding,
																				servicePartitionResolver: partitionResolver);
			ServicePartitionKey servicePartition;
			if (serviceType == ServiceType.STATEFUL_SERVICE)
			{
				servicePartition = new ServicePartitionKey(0);
			}
			else
			{
				servicePartition = ServicePartitionKey.Singleton;
			}

			TContract client = (TContract)Activator.CreateInstance(typeof(TClient), new object[] { wcfClientFactory, serviceUri, servicePartition });
			//CheckIsAlive(client, serviceUri);

			return client;
		}
	}
}
