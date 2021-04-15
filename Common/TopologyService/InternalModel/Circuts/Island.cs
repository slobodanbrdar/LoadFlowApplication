namespace Common.TopologyService.InternalModel.Circuts
{
	public class MPIsland : MPCircuit
	{
		private MPRoot masterRoot;                      //koren kome pripada
		public bool validFlag;

		public MPIsland(long lid)
			: base(lid)
		{
			this.masterRoot = null;
			this.validFlag = true;
		}

		public long? SourceObject
		{
			get { return sourceObject; }
			set { sourceObject = value; }
		}

		public MPRoot MasterRoot
		{
			get { return masterRoot; }
			set { masterRoot = value; }
		}


		public override string ToString()
		{
			return "Island";
		}



	}
}
