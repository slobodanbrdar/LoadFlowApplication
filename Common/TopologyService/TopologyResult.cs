using Common.TopologyService.InternalModel.GraphElems.Branch;
using Common.TopologyService.InternalModel.GraphElems.Node;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.TopologyService
{
	[Serializable]
    [KnownType(typeof(MPNode))]
    [KnownType(typeof(MPRootNode))]
    [KnownType(typeof(MPBusNode))]
    [KnownType(typeof(MPConnectivityNode))]
    [KnownType(typeof(MPBranch))]
    [KnownType(typeof(MPLine))]
    [KnownType(typeof(MPConnectivityBranch))]
    [DataContract]
    public class TopologyResult
    {
        [DataMember]
        public List<MPNode> Nodes { get; set; }
        [DataMember]
        public List<MPBranch> Branches { get; set; }

        //public TopologyResult() { }

        public TopologyResult(List<MPNode> nodes, List<MPBranch> branches)
        {
            Nodes = nodes;
            Branches = branches;
        }
    }
}
