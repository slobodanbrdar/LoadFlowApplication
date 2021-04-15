using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	[DataContract]
	public class ExecutionReport
	{
		[DataMember]
		public string Message { get; set; }

		[DataMember]
		public ExecutionStatus Status { get; set; }

		public ExecutionReport()
		{

		}
	}
}
