using Common.TopologyService.ExtendedClassess;
using System;

namespace Common.TopologyService.InternalModel.GraphElems.Node
{
	[Serializable]
    public class MPConnectivityNode : MPNode
    {
        public MPConnectivityNode(long lid, EPhaseCode phaseCode)
            : base(lid, phaseCode)
        {
        }

        public override string ToString()
        {
            return "ConnectivityNode";
        }
    }
}
