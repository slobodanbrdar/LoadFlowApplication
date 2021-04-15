using System;
using System.Collections.Generic;

namespace Common.TopologyService.TopologyAnalyzer.AuxiliaryStructures
{
	public abstract class CVisitedGraphElem
    {
        public bool[] Visited;
        public long[] OwnerCircuit;
        public bool Loop { get; set; }
        public long Lid;

        public CVisitedGraphElem(long lid, bool isVisited)
        {
            this.Visited = new bool[3] { isVisited, isVisited, isVisited };
            this.OwnerCircuit = new long[3];
            this.Lid = lid;
        }
    }

    [Serializable]
    public class CVisitedBranch : CVisitedGraphElem
    {
        public long[] DownNode;
        public long[] UpNode;

        public long[] NextInRoot;
        public long[] PrevInRoot;

        public CVisitedBranch(long lid, bool visited = false)
            : base(lid, visited)
        {
            this.NextInRoot = new long[3];
            this.PrevInRoot = new long[3];
            this.DownNode = new long[3];
            this.UpNode = new long[3];
        }
    }

    public class CVisitedNode : CVisitedGraphElem
    {
        public List<long>[] Parents;
        public List<long>[] Children;
        public CVisitedNode(long lid, bool visited = false)
            : base(lid, visited)
        {
            this.Parents = new List<long>[3] { new List<long>(3), new List<long>(3), new List<long>(3) };
            this.Children = new List<long>[3] { new List<long>(5), new List<long>(5), new List<long>(5) };
        }


    }
}
