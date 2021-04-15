using Microsoft.ServiceFabric.Services.Remoting;
using Common.TopologyService;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.ModelManagerInterfaces
{
	[ServiceContract]
	public interface ITopologyModelAccessContract : IService
	{
		[OperationContract]
		Task<TopologyResult> GetTopologyResult();
	}
}
