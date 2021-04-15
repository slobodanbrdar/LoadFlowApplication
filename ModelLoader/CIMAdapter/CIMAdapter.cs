using System;
using System.IO;
using System.Reflection;
using System.Threading;
using CIM.Model;
using CIMParser;
using FTN.ESI.SIMES.CIM.CIMAdapter.Importer;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using Common;
using Common.NetworkModelService.GenericDataAccess;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Client;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Common.GDAInterfaces;
using Common.Clients;
using System.Threading.Tasks;

namespace FTN.ESI.SIMES.CIM.CIMAdapter
{
	public class CIMAdapter
	{
  //      private NetworkModelGDAProxy gdaQueryProxy = null;
       
		//public CIMAdapter()
		//{
		//}

  //      private NetworkModelGDAProxy GdaQueryProxy
  //      {
  //          get
  //          {
  //              if (gdaQueryProxy != null)
  //              {
  //                  gdaQueryProxy.Abort();
  //                  gdaQueryProxy = null;
  //              }

  //              gdaQueryProxy = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
  //              gdaQueryProxy.Open();

  //              return gdaQueryProxy;
  //          }
  //      }

		public Delta CreateDelta(Stream extract, SupportedProfiles extractType, out string log)
		{
			Delta nmsDelta = null;
			ConcreteModel concreteModel = null;
			Assembly assembly = null;
			string loadLog = string.Empty;
			string transformLog = string.Empty;

			if (LoadModelFromExtractFile(extract, extractType, ref concreteModel, ref assembly, out loadLog))
			{
				DoTransformAndLoad(assembly, concreteModel, extractType, out nmsDelta, out transformLog);
			}
			log = string.Concat("Load report:\r\n", loadLog, "\r\nTransform report:\r\n", transformLog);

			return nmsDelta;
		}

		public async Task<string> ApplyUpdates(Delta delta)
		{
			Binding binding = WcfUtility.CreateTcpClientBinding();
			ServicePartitionResolver servicePartitionResolver = new ServicePartitionResolver(() =>
				new FabricClient());
			WcfCommunicationClientFactory<INetworkModelGDAContract> communicationClientFactory = new WcfCommunicationClientFactory<INetworkModelGDAContract>(binding, servicePartitionResolver: servicePartitionResolver);

			Uri uri = new Uri("fabric:/LoadFlowApplication/NMSGdaService");
			NetworkModelGdaClient networkModelGdaClient = new NetworkModelGdaClient(communicationClientFactory, uri, new ServicePartitionKey(0));

			string updateResult = "Apply Updates Report:\r\n";
			System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			if ((delta != null) && (delta.NumberOfOperations != 0))
			{
				//// NetworkModelService->ApplyUpdates
				//updateResult = GdaQueryProxy.ApplyUpdate(delta).ToString();
				try
				{
					updateResult = (await networkModelGdaClient.InvokeWithRetryAsync(client => client.Channel.ApplyUpdate(delta))).ToString();
				}
				catch (Exception e)
				{
					string message = e.Message;
				}
			}

			Thread.CurrentThread.CurrentCulture = culture;
			return updateResult;
		}


		private bool LoadModelFromExtractFile(Stream extract, SupportedProfiles extractType, ref ConcreteModel concreteModelResult, ref Assembly assembly, out string log)
		{
			bool valid = false;
			log = string.Empty;

			System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			try
			{
				ProfileManager.LoadAssembly(extractType, out assembly);
				if (assembly != null)
				{
					CIMModel cimModel = new CIMModel();
					CIMModelLoaderResult modelLoadResult = CIMModelLoader.LoadCIMXMLModel(extract, ProfileManager.Namespace, out cimModel);
					if (modelLoadResult.Success)
					{
						concreteModelResult = new ConcreteModel();
						ConcreteModelBuilder builder = new ConcreteModelBuilder();
						ConcreteModelBuildingResult modelBuildResult = builder.GenerateModel(cimModel, assembly, ProfileManager.Namespace, ref concreteModelResult);

						if (modelBuildResult.Success)
						{
							valid = true;
						}
						log = modelBuildResult.Report.ToString();
					}
					else
					{
						log = modelLoadResult.Report.ToString();
					}
				}
			}
			catch (Exception e)
			{
				log = e.Message;
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = culture;
			}
			return valid;
		}

		private bool DoTransformAndLoad(Assembly assembly, ConcreteModel concreteModel, SupportedProfiles extractType, out Delta nmsDelta, out string log)
		{
			nmsDelta = null;
			log = string.Empty;
			bool success = false;
			try
			{
				LogManager.Log(string.Format("Importing {0} data...", extractType), LogLevel.Info);

				switch (extractType)
				{
					//case SupportedProfiles.PowerTransformer:
					//	{
					//		// transformation to DMS delta					
					//		TransformAndLoadReport report = PowerTransformerImporter.Instance.CreateNMSDelta(concreteModel);

					//		if (report.Success)
					//		{
					//			nmsDelta = PowerTransformerImporter.Instance.NMSDelta;
					//			success = true;
					//		}
					//		else
					//		{
					//			success = false;
					//		}
					//		log = report.Report.ToString();
					//		PowerTransformerImporter.Instance.Reset();

					//		break;
					//	}
					case  SupportedProfiles.LoadFlowProject:
						{
							TransformAndLoadReport report = LoadFlowImporter.Instance.CreateNMSDelta(concreteModel);
							if (report.Success)
							{
								nmsDelta = LoadFlowImporter.Instance.NMSDelta;
								success = true;
							}
							else
							{
								success = false;
							}
							log = report.Report.ToString();
							LoadFlowImporter.Instance.Reset();

							break;
						}
					default:
						{
							LogManager.Log(string.Format("Import of {0} data is NOT SUPPORTED.", extractType), LogLevel.Warning);
							break;
						}
				}

				return success;
			}
			catch (Exception ex)
			{
				LogManager.Log(string.Format("Import unsuccessful: {0}", ex.StackTrace), LogLevel.Error);
				return false;
			}
		}

	}
}
