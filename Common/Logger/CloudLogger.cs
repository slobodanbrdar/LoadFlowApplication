using System;
using System.Fabric;

namespace Common.Logger
{
	internal class CloudLogger : ILogger
	{
		private readonly LogLevel sharedLogLevel = LogLevel.Information;

		private readonly string sourceName;

		private IServiceEventTracing serviceEventTracing;
		private ServiceContext serviceContext;

		internal CloudLogger(IServiceEventTracing serviceEventTracing, ServiceContext serviceContext, string sourceName)
		{
			this.serviceEventTracing = serviceEventTracing;
			this.serviceContext = serviceContext;
			this.sourceName = sourceName;
		}

		public void LogVerbose(string message, Exception e = null)
		{
			if (sharedLogLevel <= LogLevel.Verbose)
			{
				LogServiceEvent("VERBOSE: " + message, e);
			}
		}

		public void LogDebug(string message, Exception e = null)
		{
			if (sharedLogLevel <= LogLevel.Debug)
			{
				LogServiceEvent("DEBUG: " + message, e);
			}
		}

		public void LogInformation(string message, Exception e = null)
		{
			if (sharedLogLevel <= LogLevel.Information)
			{
				LogServiceEvent("INFORMATION: " + message, e);
			}
		}

		public void LogWarning(string message, Exception e = null)
		{
			if (sharedLogLevel <= LogLevel.Warning)
			{
				LogServiceEvent("WARNING: " + message, e);
			}
		}

		public void LogError(string message, Exception e = null)
		{
			if (sharedLogLevel <= LogLevel.Error)
			{
				LogServiceEvent("ERROR: " + message, e);
			}
		}

		public void LogFatal(string message, Exception e = null)
		{
			if (sharedLogLevel <= LogLevel.Fatal)
			{
				LogServiceEvent("FATAL: " + message, e);
			}
		}


		public void SetServiceContext(ServiceContext serviceContext)
		{
			this.serviceContext = serviceContext;
		}

		public void SetServiceEventTracing(IServiceEventTracing serviceEventTracing)
		{
			this.serviceEventTracing = serviceEventTracing;
		}

		private void LogServiceEvent(string message, Exception e = null)
		{
			if (this.serviceEventTracing == null || this.serviceContext == null)
			{
				return;
			}

			if (e == null)
			{
				this.serviceEventTracing.UniversalServiceMessage(this.serviceContext, message);
			}
			else
			{
				this.serviceEventTracing.UniversalServiceMessage(this.serviceContext, $"{message}{Environment.NewLine}Exception Message: {e.Message}");
			}
		}
	}
}
