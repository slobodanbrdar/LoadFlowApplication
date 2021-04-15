using System;
using System.Runtime.Serialization;

namespace Common.TopologyService.InternalModel
{
	[Serializable]
	[DataContract]
	public abstract class MPIdentifiedObject
	{
		private long lid;
		[DataMember]
		public long Lid { get { return lid; } set { lid = value; } }

		public MPIdentifiedObject(long id)
		{
			lid = id;
		}

	}
}
