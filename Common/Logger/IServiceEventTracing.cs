using System.Fabric;

namespace Common.Logger
{
	public interface IServiceEventTracing
	{
		void UniversalServiceMessage(ServiceContext serviceContext, string message);
	}
}
