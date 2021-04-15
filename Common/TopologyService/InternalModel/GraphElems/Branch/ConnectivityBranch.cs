using Common.TopologyService.ExtendedClassess;
using System;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModel.GraphElems.Branch
{
	[Serializable]
    [DataContract]
    public class MPConnectivityBranch : MPBranch
    {
        [DataMember]
		public long OriginalBranchLid { get; private set; }
		public MPConnectivityBranch(long lid, long originalBranchLid, EPhaseCode phaseCode, long upNode, long downNode)
            : base(lid, phaseCode, upNode, downNode)
        {
            OriginalBranchLid = originalBranchLid;
        }

        public override string ToString()
        {
            return "ConnectivityBranch";
        }
    }
}
