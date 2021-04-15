using Common.TopologyService.ExtendedClassess;
using System;

namespace Common.TopologyService.InternalModel.GraphElems.Branch
{
	[Serializable]
    public class MPLine : MPBranch
    {
        public MPLine(long lid, EPhaseCode phaseCode, long upNode, long downNode)
            : base(lid, phaseCode, upNode, downNode)
        {
        }

        public override string ToString()
        {
            return "Line";
        }
    }
}
