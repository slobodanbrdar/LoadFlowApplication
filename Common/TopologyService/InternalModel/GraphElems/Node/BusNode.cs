using Common.TopologyService.ExtendedClassess;
using System;

namespace Common.TopologyService.InternalModel.GraphElems.Node
{
	[Serializable]
    public class MPBusNode : MPNode
    {
        public MPBusNode(long lid, EPhaseCode phaseCode)
            : base(lid, phaseCode)
        {
        }

        public override string ToString()
        {
            return "BusNode";
        }
    }
}
