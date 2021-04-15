using System;
using System.Collections.Generic;

namespace Common.TopologyService.InternalModel.Circuts
{
	[Serializable]
	public class MPRoot : MPCircuit
	{
		#region Fields

		public long MasterRoot;

		public List<long> SlaveRoots;

		public bool UpToDate;

		#endregion


		public long? SourceObject
		{
			get { return sourceObject; }
		}


		public MPRoot(long lid, long nodeLid)
			: base(lid)
		{
			this.UpToDate = false;
			this.MasterRoot = 0;
			this.SlaveRoots = new List<long>();
			this.sourceObject = nodeLid;
		}

		public void InitTopology()
		{
			GraphElems.Clear();
			MasterRoot = 0;
			SlaveRoots.Clear();
		}


		public override string ToString()
		{
			return "Root";
		}


	}
}
