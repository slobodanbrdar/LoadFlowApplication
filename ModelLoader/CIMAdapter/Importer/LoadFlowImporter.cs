using CIM.Model;
using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using System;
using System.Collections.Generic;


namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	public class LoadFlowImporter
	{

		private static LoadFlowImporter lfImporter = null;
		private static object singletoneLock = new object();

		private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;


		#region Properties
		public static LoadFlowImporter Instance
		{
			get
			{
				if (lfImporter == null)
				{
					lock (singletoneLock)
					{
						if (lfImporter == null)
						{
							lfImporter = new LoadFlowImporter();
							lfImporter.Reset();
						}
					}
				}
				return lfImporter;
			}
		}

		public Delta NMSDelta
		{
			get
			{
				return delta;
			}
		}
		#endregion

		public void Reset()
		{
			concreteModel = null;
			delta = new Delta();
			importHelper = new ImportHelper();
			report = null;
		}

		public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel)
		{
			LogManager.Log("Importing PowerTransformer Elements...", LogLevel.Info);
			report = new TransformAndLoadReport();
			concreteModel = cimConcreteModel;
			delta.ClearDeltaOperations();

			if ((concreteModel != null) && (concreteModel.ModelMap != null))
			{
				try
				{
					ConvertModelAndPopulateDelta();
				}
				catch (Exception ex)
				{
					string message = string.Format("{0} - ERROR in data import - {1}", DateTime.Now, ex.Message);
					LogManager.Log(message);
					report.Report.AppendLine(ex.Message);
					report.Success = false;
				}
			}

			LogManager.Log("Importing PowerTransformer Elements - END.", LogLevel.Info);
			return report;
		}

		private void ConvertModelAndPopulateDelta()
		{
			LogManager.Log("Loading elements and creating delta...", LogLevel.Info);

			ImportBaseVoltages();
			ImportConnectivityNodes();
			ImportEnergyConsumers();
			ImportEnergySources();
			ImportPowerTransformers();
			ImportPowerTransformerEnds();
			ImportSwitches();
			ImportACLineSegments();
			ImportTerminals();

			LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

		#region Import
		private void ImportBaseVoltages()
		{
			SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.BaseVoltage");
			if (cimBaseVoltages != null)
			{
				foreach(KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
				{
					FTN.BaseVoltage cimBaseVoltage = cimBaseVoltagePair.Value as FTN.BaseVoltage;

					ResourceDescription rd = CreateBaseVoltageResourceDecription(cimBaseVoltage);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("BaseVoltage ID = ").Append(cimBaseVoltage.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("BaseVoltage ID = ").Append(cimBaseVoltage.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateBaseVoltageResourceDecription(BaseVoltage cimBaseVoltage)
		{
			ResourceDescription rd = null;
			if (cimBaseVoltage != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.BASEVOLTAGE, importHelper.CheckOutIndexForDMSType(DMSType.BASEVOLTAGE));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimBaseVoltage.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateBaseVoltageProperties(cimBaseVoltage, rd);
			}
			return rd;
		}

		private void ImportConnectivityNodes()
		{
			SortedDictionary<string, object> cimConnectivityNodes = concreteModel.GetAllObjectsOfType("FTN.ConnectivityNode");
			if (cimConnectivityNodes != null)
			{
				foreach (KeyValuePair<string, object> cimConnectivityNodePair in cimConnectivityNodes)
				{
					FTN.ConnectivityNode cimConnectivityNode = cimConnectivityNodePair.Value as FTN.ConnectivityNode;

					ResourceDescription rd = CreateConnectivityNodeResourceDecription(cimConnectivityNode);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("ConnectiviyNode ID = ").Append(cimConnectivityNode.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ConnectivityNode ID = ").Append(cimConnectivityNode.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateConnectivityNodeResourceDecription(ConnectivityNode cimConnectivityNode)
		{
			ResourceDescription rd = null;
			if (cimConnectivityNode != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.CONNECTIVITYNODE, importHelper.CheckOutIndexForDMSType(DMSType.CONNECTIVITYNODE));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimConnectivityNode.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateConnectivityNodeProperties(cimConnectivityNode, rd);
			}
			return rd;
		}

		private void ImportEnergyConsumers()
		{
			SortedDictionary<string, object> cimEnergyConsumers = concreteModel.GetAllObjectsOfType("FTN.EnergyConsumer");
			if (cimEnergyConsumers != null)
			{
				foreach (KeyValuePair<string, object> cimEnergyConsumerPair in cimEnergyConsumers)
				{
					FTN.EnergyConsumer cimEnergyConsumer = cimEnergyConsumerPair.Value as FTN.EnergyConsumer;

					ResourceDescription rd = CreateEnergyConsumerResourceDecription(cimEnergyConsumer);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("EnergyConsumer ID = ").Append(cimEnergyConsumer.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateEnergyConsumerResourceDecription(EnergyConsumer cimEnergyConsumer)
		{
			ResourceDescription rd = null;
			if (cimEnergyConsumer != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGYCONSUMER, importHelper.CheckOutIndexForDMSType(DMSType.ENERGYCONSUMER));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimEnergyConsumer.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateEnergyConsumerProperties(cimEnergyConsumer, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportEnergySources()
		{
			SortedDictionary<string, object> cimEnergySources = concreteModel.GetAllObjectsOfType("FTN.EnergySource");
			if (cimEnergySources != null)
			{
				foreach (KeyValuePair<string, object> cimEnergySourcePair in cimEnergySources)
				{
					FTN.EnergySource cimEnergySource = cimEnergySourcePair.Value as FTN.EnergySource;

					ResourceDescription rd = CreateEnergySourceResourceDecription(cimEnergySource);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("EnergySource ID = ").Append(cimEnergySource.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("EnergySource ID = ").Append(cimEnergySource.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateEnergySourceResourceDecription(EnergySource cimEnergySource)
		{
			ResourceDescription rd = null;
			if (cimEnergySource != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ENERGYSOURCE, importHelper.CheckOutIndexForDMSType(DMSType.ENERGYSOURCE));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimEnergySource.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateEnergySourceProperties(cimEnergySource, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportPowerTransformers()
		{
			SortedDictionary<string, object> cimPowerTransformers = concreteModel.GetAllObjectsOfType("FTN.PowerTransformer");
			if (cimPowerTransformers != null)
			{
				foreach (KeyValuePair<string, object> cimPowerTransformerPair in cimPowerTransformers)
				{
					FTN.PowerTransformer cimPowerTransformer = cimPowerTransformerPair.Value as FTN.PowerTransformer;

					ResourceDescription rd = CreatePowerTransformerResourceDecription(cimPowerTransformer);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("PowerTransformer ID = ").Append(cimPowerTransformer.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("PowerTransformer ID = ").Append(cimPowerTransformer.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreatePowerTransformerResourceDecription(PowerTransformer cimPowerTransformer)
		{
			ResourceDescription rd = null;
			if (cimPowerTransformer != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.POWERTRANSFORMER, importHelper.CheckOutIndexForDMSType(DMSType.POWERTRANSFORMER));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimPowerTransformer.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulatePowerTransformerProperties(cimPowerTransformer, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportPowerTransformerEnds()
		{
			SortedDictionary<string, object> cimPowerTransformerEnds = concreteModel.GetAllObjectsOfType("FTN.PowerTransformerEnd");
			if (cimPowerTransformerEnds != null)
			{
				foreach (KeyValuePair<string, object> cimPowerTransformerEndPair in cimPowerTransformerEnds)
				{
					FTN.PowerTransformerEnd cimPowerTransformerEnd = cimPowerTransformerEndPair.Value as FTN.PowerTransformerEnd;

					ResourceDescription rd = CreatePowerTransformerEndResourceDecription(cimPowerTransformerEnd);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("PowerTransformerEnd ID = ").Append(cimPowerTransformerEnd.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("PowerTransformerEnd ID = ").Append(cimPowerTransformerEnd.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreatePowerTransformerEndResourceDecription(PowerTransformerEnd cimPowerTransformerEnd)
		{
			ResourceDescription rd = null;
			if (cimPowerTransformerEnd != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.PTRANSFORMEREND, importHelper.CheckOutIndexForDMSType(DMSType.PTRANSFORMEREND));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimPowerTransformerEnd.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulatePowerTransformerEndProperties(cimPowerTransformerEnd, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportSwitches()
		{
			SortedDictionary<string, object> cimSwitches = concreteModel.GetAllObjectsOfType("FTN.Switch");
			if (cimSwitches != null)
			{
				foreach (KeyValuePair<string, object> cimSwitchPair in cimSwitches)
				{
					FTN.Switch cimSwitch = cimSwitchPair.Value as FTN.Switch;

					ResourceDescription rd = CreateSwitchResourceDecription(cimSwitch);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("Switch ID = ").Append(cimSwitch.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("Switch ID = ").Append(cimSwitch.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateSwitchResourceDecription(Switch cimSwitch)
		{
			ResourceDescription rd = null;
			if (cimSwitch != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SWITCH, importHelper.CheckOutIndexForDMSType(DMSType.SWITCH));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimSwitch.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateSwitchProperties(cimSwitch, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportACLineSegments()
		{
			SortedDictionary<string, object> cimACLineSegmetns = concreteModel.GetAllObjectsOfType("FTN.ACLineSegment");
			if (cimACLineSegmetns != null)
			{
				foreach (KeyValuePair<string, object> cimACLineSegmentPair in cimACLineSegmetns)
				{
					FTN.ACLineSegment cimACLineSegment = cimACLineSegmentPair.Value as FTN.ACLineSegment;

					ResourceDescription rd = CreateACLineSegmentResourceDecription(cimACLineSegment);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("ACLineSegment ID = ").Append(cimACLineSegment.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ACLineSegment ID = ").Append(cimACLineSegment.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateACLineSegmentResourceDecription(ACLineSegment cimACLineSegment)
		{
			ResourceDescription rd = null;
			if (cimACLineSegment != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.ACLINESEGMENT, importHelper.CheckOutIndexForDMSType(DMSType.ACLINESEGMENT));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimACLineSegment.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateACLineSegmentProperties(cimACLineSegment, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportTerminals()
		{
			SortedDictionary<string, object> cimTerminals = concreteModel.GetAllObjectsOfType("FTN.Terminal");
			if (cimTerminals != null)
			{
				foreach (KeyValuePair<string, object> cimTerminalPair in cimTerminals)
				{
					FTN.Terminal cimTerminal = cimTerminalPair.Value as FTN.Terminal;

					ResourceDescription rd = CreateTerminalResourceDecription(cimTerminal);

					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("Terminal ID = ").Append(cimTerminal.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateTerminalResourceDecription(Terminal cimTerminal)
		{
			ResourceDescription rd = null;
			if (cimTerminal != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.TERMINAL, importHelper.CheckOutIndexForDMSType(DMSType.TERMINAL));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimTerminal.ID, gid);

				////populate ResourceDescription
				LoadFlowConverter.PopulateTerminalProperties(cimTerminal, rd, importHelper, report);
			}
			return rd;
		}

		#endregion
	}
}
