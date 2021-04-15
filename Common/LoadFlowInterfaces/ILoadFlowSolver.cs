using Microsoft.ServiceFabric.Services.Remoting;
using OpenDSSengine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.LoadFlowInterfaces
{
	[ServiceContract]
	public interface ILoadFlowSolver : IService
	{
		[OperationContract]
		Task SolveLoadFlow();
	}
}
