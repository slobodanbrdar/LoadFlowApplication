using Common.Logger;
using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using System;
using System.Collections.Generic;

namespace NMSGdaImplementation.GDA
{
	public class ResourceIterator
	{
		private ILogger logger;
		protected ILogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		private List<long> globalDs = new List<long>();
		private Dictionary<DMSType, List<ModelCode>> class2PropertyIDs = new Dictionary<DMSType, List<ModelCode>>();

		private int lastReadIndex = 0; // index of the last read resource description
		private int maxReturnNo = 5000;

		public int LastReadIndex
		{
			get
			{
				return lastReadIndex;
			}
			private set
			{
				lastReadIndex = value;
			}
		}

		public int MaxReturnNo
		{
			get
			{
				return maxReturnNo;
			}
			private set
			{
				maxReturnNo = value;
			}
		}


		public ResourceIterator()
		{
		}

		public ResourceIterator(List<long> globalIDs, Dictionary<DMSType, List<ModelCode>> class2PropertyIDs)
		{
			this.globalDs = globalIDs;
			this.class2PropertyIDs = class2PropertyIDs;
		}

		public int ResourcesLeft()
		{
			return globalDs.Count - lastReadIndex;
		}

		public int ResourcesTotal()
		{
			return globalDs.Count;
		}

		public List<ResourceDescription> Next(int n, NetworkModel networkModel)
		{
			try
			{
				if (n < 0)
				{
					return null;
				}

				if (n > maxReturnNo)
				{
					n = maxReturnNo;
				}

				List<long> resultIDs;

				if (ResourcesLeft() < n)
				{
					resultIDs = globalDs.GetRange(lastReadIndex, globalDs.Count - lastReadIndex);
					lastReadIndex = globalDs.Count;
				}
				else
				{
					resultIDs = globalDs.GetRange(lastReadIndex, n);
					lastReadIndex += n;
				}

				List<ResourceDescription> result = CollectData(resultIDs, networkModel);

				return result;
			}
			catch (Exception ex)
			{
				string message = string.Format("Failed to get next set of ResourceDescription iterators. {0}", ex.Message);
				Logger.LogError(message);
				throw new Exception(message);
			}
		}

		public List<ResourceDescription> GetRange(int index, int n, NetworkModel networkModel)
		{
			try
			{
				if (n > maxReturnNo)
				{
					n = maxReturnNo;
				}

				List<long> resultIDs = globalDs.GetRange(index, n);

				List<ResourceDescription> result = CollectData(resultIDs, networkModel);

				return result;
			}
			catch (Exception ex)
			{
				string message = string.Format("Failed to get range of ResourceDescription iterators. index:{0}, count:{1}. {2}", index, n, ex.Message);
				Logger.LogError(message);
				throw new Exception(message);
			}
		}

		public void Rewind()
		{
			lastReadIndex = 0;
		}

		private List<ResourceDescription> CollectData(List<long> resultIDs, NetworkModel networkModel)
		{
			try
			{
				List<ResourceDescription> result = new List<ResourceDescription>();

				List<ModelCode> propertyIds = null;
				foreach (long globalId in resultIDs)
				{
					propertyIds = class2PropertyIDs[(DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(globalId)];
					result.Add(networkModel.GetValues(globalId, propertyIds));
				}

				return result;
			}
			catch (Exception ex)
			{
				Logger.LogError($"Collecting ResourceDescriptions failed. Exception:{ex.Message}", ex);
				throw;
			}
		}
	}
}
