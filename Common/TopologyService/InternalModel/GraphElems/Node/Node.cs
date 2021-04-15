using Common.TopologyService.ExtendedClassess;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModel.GraphElems.Node
{
	[Serializable]
	[DataContract]
	public abstract class MPNode : MPGraphElem
	{

		#region fields

		[DataMember]
		public List<long>[] Parents { get; set; }
		[DataMember]
		public List<long>[] Children { get; set; }

		#endregion

		public MPNode(long lid, EPhaseCode phaseCode)
			: base(lid, phaseCode)
		{
			this.Parents = null;
			this.Children = null;
		}

		

		public void InitTopology()
		{
			Parents = null;
			Children = null;
			OwnerCircuit = null;
			ActivePhases = EPhaseCode.UNKNOWN;
			Marker = EEnergizationStatus.TA_UNKNOWN;
		}

	}
}
