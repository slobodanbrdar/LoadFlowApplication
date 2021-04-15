using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public enum ServiceType
    {
        [EnumMember]
        STATEFUL_SERVICE = 1,

        [EnumMember]
        STATELESS_SERVICE = 2,
    }

    [DataContract]
    public enum ExecutionStatus
	{
        [EnumMember]
        ERROR = 1,

        [EnumMember]
        SUCCESS = 2,
	}
}
