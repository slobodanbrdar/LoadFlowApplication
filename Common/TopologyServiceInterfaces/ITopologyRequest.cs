using Common.TopologyService;
using Common.TopologyService.InternalModelBuilder;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.TopologyServiceInterfaces
{
	[ServiceContract]
	public interface ITopologyRequest : IService
	{
		[OperationContract]
		Task<IEnumerable<TopologyResult>> AnalyzeTopology(CModelFramework internalModel);
	}
}
