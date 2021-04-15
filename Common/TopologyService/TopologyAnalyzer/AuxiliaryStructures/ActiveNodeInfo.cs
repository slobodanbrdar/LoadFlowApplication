namespace Common.TopologyService.TopologyAnalyzer.AuxiliaryStructures
{
	public class ActiveNodeInfo
    {
        public long NodeIndex;
        public long PhaseIndex;

        public ActiveNodeInfo(long nodeIndex, long phaseIndex)
        {
            NodeIndex = nodeIndex;
            PhaseIndex = phaseIndex;

        }
    }
}
