using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.LoadFlowInterfaces
{
	[ServiceContract]
	public interface ILoadFlowManagerContract : IService
	{
		[OperationContract]
		Task StartLoadFlowSolving(); 
	}
}
