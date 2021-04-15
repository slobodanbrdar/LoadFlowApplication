using Common.TopologyService.ExtendedClassess;
using System;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModel.GraphElems.Branch
{
	[Serializable]
    [DataContract]
    public abstract class MPBranch : MPGraphElem
    {
        // Nodes
        [DataMember]
        private long[] endNodes;                    // Nodes on the end of branch
        [DataMember]
        public long[] EndNodes { get { return endNodes; } set { endNodes = value; }  }
        // Layers
        [DataMember]
        public long[] NextInRoot { get; set; }
        [DataMember]
        public long[] PrevInRoot { get; set; }

        // Stream direction
        [DataMember]
        public long[] DownNode { get; set; }
        [DataMember]
        public long[] UpNode { get; set; }
        [DataMember]
        public bool BothDirection { get; set; } 

        public MPBranch(long lid, EPhaseCode phaseCode, long firstNode, long secondNode)
            : base(lid, phaseCode)
        {
            this.endNodes = new long[2] { firstNode, secondNode };
            this.NextInRoot = new long[3];
            this.PrevInRoot = new long[3];
            this.DownNode = new long[3];
            this.UpNode = new long[3];
            this.BothDirection = false;
        }

        public virtual void InitTopology()
        {
            OwnerCircuit = null;
            ActivePhases = EPhaseCode.UNKNOWN;
            Marker = EEnergizationStatus.TA_UNKNOWN;
            NextInRoot = null;
            PrevInRoot = null;
            DownNode = null;
            UpNode = null;
            BothDirection = false;
        }


        public override string ToString()
        {
            return "Branch";
        }
    }
}
