using Common.TopologyService.ExtendedClassess;

namespace Common.TopologyService.InternalModel.GraphElems.Branch
{
	public class MPJumper : MPBranch
    {

        public MPJumper(long lid, EPhaseCode phaseCode, long firstNode, long secondNode) :
            base(lid, phaseCode, firstNode, secondNode)
        {
        }

        public override string ToString()
        {
            return "Jumper";
        }

    }
}
