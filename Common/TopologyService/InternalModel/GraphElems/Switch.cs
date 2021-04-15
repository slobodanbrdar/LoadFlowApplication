using Common.TopologyService.ExtendedClassess;
using System;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModel.GraphElems
{
	[Serializable]
    [DataContract]
    public class MPSwitchDevice : MPGraphElem
    {
        [DataMember]
        public EPhaseCode State { get; set; }
        [DataMember]
        public long EndBranch { get; private set; }
        [DataMember]
        public long EndNode { get; private set; }
        [DataMember]
        public long StateBranch { get; set; }

        public MPSwitchDevice(long lid, long branchLid, long nodeLid, EPhaseCode phaseCode, EPhaseCode state)
            : base(lid, phaseCode)
        {
            this.State = state;
            this.EndBranch = branchLid;
            this.EndNode = nodeLid;
        }

        public override string ToString()
        {
            return "Bay";
        }


    }
}
