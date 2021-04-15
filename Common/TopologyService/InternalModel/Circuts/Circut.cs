using System;
using System.Collections.Generic;

namespace Common.TopologyService.InternalModel.Circuts
{
	[Serializable]
	public class MPCircuit : MPIdentifiedObject
	{
		protected long? sourceObject;                               //elemenat(grana ili cvor) kojim koren ili ostrvo pocinje

		public List<long> GraphElems { get; private set; }

		public MPCircuit(long lid)
			: base(lid)
		{
			sourceObject = null;
			GraphElems = new List<long>(100);
		}

		public override string ToString()
		{
			return "Circuit";
		}


	}
}
