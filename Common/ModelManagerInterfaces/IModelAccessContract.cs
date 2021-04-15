using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.ModelManagerInterfaces
{
	[ServiceContract]
	public interface IModelAccessContract : IService
	{
		[OperationContract]
		Task<ExecutionReport> InitializeTopology();

		[OperationContract]
		Task<ExecutionReport> GetOpenDSSScript();
	}
}
