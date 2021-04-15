using Common.TopologyService.ExtendedClassess;
using System;

namespace Common.TopologyService.InternalModel.GraphElems.Node
{
	[Serializable]
    public class MPRootNode : MPNode
    {
        public MPRootNode(long lid, long root, EPhaseCode phaseCode)
            : base(lid, phaseCode)
        {
            OwnerCircuit = new long[] { root, root, root };
        }

        public override string ToString()
        {
            return "RootNode";
        }
    }
}
