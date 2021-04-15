using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Clients
{
	public class WcfSeviceFabricClientBase<TContract> : ServicePartitionClient<WcfCommunicationClient<TContract>> where TContract : class, IService
	{
		public WcfSeviceFabricClientBase(WcfCommunicationClientFactory<TContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition, string listenerName)
			: base(clientFactory, serviceName, servicePartition, TargetReplicaSelector.Default, listenerName)
		{

		}
	}
}
