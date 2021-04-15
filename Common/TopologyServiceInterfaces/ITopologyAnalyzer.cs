using Common.TopologyService;
using Common.TopologyService.InternalModelBuilder;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.TopologyServiceInterfaces
{
	[ServiceContract]
    public interface ITopologyAnalyzer : IService
    {
        [OperationContract]
        Task<TopologyResult> AnalyzeTopology(CModelFramework internalModel);
    }
}
