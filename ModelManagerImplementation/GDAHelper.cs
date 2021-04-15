using Common.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.GDAInterfaces;
using Common.NetworkModelService.GenericDataAccess;
using Common.NetworkModelService;
using Common.Logger;

namespace ModelManagerImplementation
{
	public class GDAHelper
	{
		private ILogger logger;
		private ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		private INetworkModelGDAContract networkModelGDAContract;
		private INetworkModelGDAContract NetworkModelGDAContract
		{
			get
			{
				networkModelGDAContract = NetworkModelGdaClient.CreateClient();
				return networkModelGDAContract;
			}
		}
		private ModelResourcesDesc resourcesDesc;

		public GDAHelper()
		{
			resourcesDesc = new ModelResourcesDesc();
		}

		public async Task<ResourceDescription> GetValues(long gid)
		{
			Logger.LogInformation($"Getting values method for gid {gid:X16} started.");

			ResourceDescription result = null;

			try
			{
				short type = ModelCodeHelper.ExtractTypeFromGlobalId(gid);
				List<ModelCode> properties = resourcesDesc.GetAllPropertyIds((DMSType)type);

				result = await NetworkModelGDAContract.GetValues(gid, properties);

				Logger.LogInformation("Getting values method successfully finished.");
				
			} 
			catch (Exception e)
			{
				string message = $"Getting values method for id {gid:X16} failed with error: {e.Message}";
				Logger.LogError(message, e);
			}

			return result;
		}

		public async Task<List<ResourceDescription>> GetExtentValues(ModelCode type)
		{
			Logger.LogInformation($"Getting extended values for type {type} started.");

			List<ResourceDescription> result = new List<ResourceDescription>();

			try
			{
				List<ModelCode> properties = resourcesDesc.GetAllPropertyIds(type);

				int iteratorId = await NetworkModelGDAContract.GetExtentValues(type, properties);
				int resourcesLeft = await NetworkModelGDAContract.IteratorResourcesLeft(iteratorId);
				int numberOfResources = 10;

				while (resourcesLeft > 0)
				{
					List<ResourceDescription> values = await NetworkModelGDAContract.IteratorNext(numberOfResources, iteratorId);
					result.AddRange(values);

					resourcesLeft = await NetworkModelGDAContract.IteratorResourcesLeft(iteratorId);
				}

				await NetworkModelGDAContract.IteratorClose(iteratorId);

				Logger.LogInformation($"Getting extended values method for type {type} successfully finished. Number of items is {result.Count}");
			}
			catch (Exception e)
			{
				string message = $"Getting extended values method for type {type} failed with error: {e.Message}";
				Logger.LogError(message, e);
			}

			return result;
		}

		public async Task<long> GetVersion()
		{
			Logger.LogInformation("Getting version method started");
			long version = -1;
			try
			{
				version = await NetworkModelGDAContract.GetVersion();
			}
			catch (Exception e)
			{
				string message = $"Getting version method failed with error: {e.Message}.";
				Logger.LogError(message, e);
			}

			return version;
		}
	}
}
