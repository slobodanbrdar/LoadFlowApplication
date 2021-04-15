using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NMSGdaService
{
	[ServiceContract]
	public interface ITest : IService
	{
		[OperationContract]
		Task<string> ReturnOK();
	}
}
