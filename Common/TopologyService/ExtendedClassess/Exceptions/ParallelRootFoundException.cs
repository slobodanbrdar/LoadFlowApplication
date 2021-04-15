using System;

namespace Common.TopologyService.ExtendedClassess.Exceptions
{
	[Serializable]
	public class ParallelRootFoundException : Exception
	{
		public ParallelRootFoundException(string message) :
			base(message)
		{

		}
	}
}
