namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	using Common.NetworkModelService;
	using Common.NetworkModelService.GenericDataAccess;
	public static class LoadFlowConverter
	{
		#region Populate ResourceDescription
		public static void PopulateIdentifiedObjectProperties(FTN.IdentifiedObject cimIdentifiedObject, ResourceDescription rd)
		{
			if ((cimIdentifiedObject != null) && (rd != null))
			{
				if (cimIdentifiedObject.MRIDHasValue)
				{
					rd.AddProperty(new Property(ModelCode.IDOBJ_MRID, cimIdentifiedObject.MRID));
				}
				if (cimIdentifiedObject.NameHasValue)
				{
					rd.AddProperty(new Property(ModelCode.IDOBJ_NAME, cimIdentifiedObject.Name));
				}
				if (cimIdentifiedObject.AliasNameHasValue)
				{
					rd.AddProperty(new Property(ModelCode.IDOBJ_ALIASNAME, cimIdentifiedObject.AliasName));
				}
			}
		}

		public static void PopulatePowerSystemResourceProperties(FTN.PowerSystemResource cimPowerSystemResource, ResourceDescription rd)
		{
			if ((cimPowerSystemResource != null) && (rd != null))
			{
				LoadFlowConverter.PopulateIdentifiedObjectProperties(cimPowerSystemResource, rd);
			}
		}


		public static void PopulateBaseVoltageProperties(FTN.BaseVoltage cimBaseVoltage, ResourceDescription rd)
		{
			if ((cimBaseVoltage != null) && (rd != null))
			{
				LoadFlowConverter.PopulateIdentifiedObjectProperties(cimBaseVoltage, rd);

				if (cimBaseVoltage.NameHasValue)
				{
					rd.AddProperty(new Property(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE, cimBaseVoltage.NominalVoltage));
				}
			}
		}

		public static void PopulatePowerTransformerEndProperties(FTN.PowerTransformerEnd cimPowerTransformerEnd, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimPowerTransformerEnd != null) && (rd != null))
			{
				LoadFlowConverter.PopulateIdentifiedObjectProperties(cimPowerTransformerEnd, rd);

				if (cimPowerTransformerEnd.ConnectionKindHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_CONNKIND, (short)GetDMSWindingConnection(cimPowerTransformerEnd.ConnectionKind)));
				}
				if (cimPowerTransformerEnd.BHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_B, cimPowerTransformerEnd.B));
				}
				if (cimPowerTransformerEnd.B0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_B0, cimPowerTransformerEnd.B0));
				}
				if (cimPowerTransformerEnd.GHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_G, cimPowerTransformerEnd.G));
				}
				if (cimPowerTransformerEnd.G0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_G0, cimPowerTransformerEnd.G));
				}
				if (cimPowerTransformerEnd.RHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_R, cimPowerTransformerEnd.R));
				}
				if (cimPowerTransformerEnd.R0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_R0, cimPowerTransformerEnd.R0));
				}
				if (cimPowerTransformerEnd.XHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_X, cimPowerTransformerEnd.X));
				}
				if (cimPowerTransformerEnd.X0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_X0, cimPowerTransformerEnd.X0));
				}
				if (cimPowerTransformerEnd.RatedSHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_RATEDS, cimPowerTransformerEnd.RatedS));
				}
				if (cimPowerTransformerEnd.RatedUHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_RATEDU, cimPowerTransformerEnd.RatedU));
				}
				if (cimPowerTransformerEnd.EndNumberHasValue)
				{
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_ENDNUMBER, cimPowerTransformerEnd.EndNumber));
				}
				if (cimPowerTransformerEnd.PowerTransformerHasValue)
				{
					long gid = importHelper.GetMappedGID(cimPowerTransformerEnd.PowerTransformer.ID);
					if(gid < 0)
					{
						report.Report.Append("WARNING: Convert ").Append(cimPowerTransformerEnd.GetType().ToString()).Append(" rdfID = \"").Append(cimPowerTransformerEnd.ID);
						report.Report.Append("\" - Failed to set reference to PowerTransformer: rdfID \"").Append(cimPowerTransformerEnd.PowerTransformer.ID).AppendLine(" \" is not mapped to GID!");
					}
					rd.AddProperty(new Property(ModelCode.PTRANSFORMEREND_POWERTRANSFORMER, gid));
				}
			}
		}

		public static void PopulateConnectivityNodeProperties(FTN.ConnectivityNode cimConnectivityNode, ResourceDescription rd)
		{
			if ((cimConnectivityNode != null) && (rd != null))
			{
				LoadFlowConverter.PopulateIdentifiedObjectProperties(cimConnectivityNode, rd);
			}
		}

		public static void PopulateTerminalProperties(FTN.Terminal cimTerminal, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimTerminal != null) && (rd != null))
			{
				LoadFlowConverter.PopulateIdentifiedObjectProperties(cimTerminal, rd);
				if (cimTerminal.PhasesHasValue)
				{
					rd.AddProperty(new Property(ModelCode.TERMINAL_PHASES, (short)GetDMSPhaseCode(cimTerminal.Phases)));
				}
				if (cimTerminal.ConductingEquipmentHasValue)
				{
					long gid = importHelper.GetMappedGID(cimTerminal.ConductingEquipment.ID);
					if (gid < 0)
					{
						report.Report.Append("WARNING: Convert ").Append(cimTerminal.GetType().ToString()).Append(" rdfID = \"").Append(cimTerminal.ID);
						report.Report.Append("\" - Failed to set reference to CondunctingEquipment: rdfID \"").Append(cimTerminal.ConductingEquipment.ID).AppendLine(" \" is not mapped to GID!");
					}
					rd.AddProperty(new Property(ModelCode.TERMINAL_CONDEQ, gid));
				}
				if (cimTerminal.ConnectivityNodeHasValue)
				{
					long gid = importHelper.GetMappedGID(cimTerminal.ConnectivityNode.ID);
					if (gid < 0)
					{
						report.Report.Append("WARNING: Convert ").Append(cimTerminal.GetType().ToString()).Append(" rdfID = \"").Append(cimTerminal.ID);
						report.Report.Append("\" - Failed to set reference to ConnectivityNode: rdfID \"").Append(cimTerminal.ConnectivityNode.ID).AppendLine(" \" is not mapped to GID!");
					}
					rd.AddProperty(new Property(ModelCode.TERMINAL_CONNECTIVITYNODE, gid));
				}
			}
		}

		public static void PopulateEquipmentProperties(FTN.Equipment cimEquipment, ResourceDescription rd)
		{
			if ((cimEquipment != null) && (rd != null))
			{
				LoadFlowConverter.PopulatePowerSystemResourceProperties(cimEquipment, rd);
			}
		}

		public static void PopulateConductingEquipmentProperties(FTN.ConductingEquipment cimConductingEquipment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimConductingEquipment != null) && (rd != null))
			{
				LoadFlowConverter.PopulateEquipmentProperties(cimConductingEquipment, rd);

				if (cimConductingEquipment.BaseVoltageHasValue)
				{
					long gid = importHelper.GetMappedGID(cimConductingEquipment.BaseVoltage.ID);
					if (gid < 0)
					{
						report.Report.Append("WARNING: Convert ").Append(cimConductingEquipment.GetType().ToString()).Append(" rdfID = \"").Append(cimConductingEquipment.ID);
						report.Report.Append("\" - Failed to set reference to BaseVoltage: rdfID \"").Append(cimConductingEquipment.BaseVoltage.ID).AppendLine(" \" is not mapped to GID!");
					}
					rd.AddProperty(new Property(ModelCode.CONDEQ_BASVOLTAGE, gid));
				}
			}
		}

		public static void PopulatePowerTransformerProperties(FTN.PowerTransformer cimPowerTransformer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimPowerTransformer != null) && (rd != null))
			{
				LoadFlowConverter.PopulateConductingEquipmentProperties(cimPowerTransformer, rd, importHelper, report);
			}
		}

		public static void PopulateConductorProperties(FTN.Conductor cimConductor, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if((cimConductor != null) && (rd != null))
			{
				LoadFlowConverter.PopulateConductingEquipmentProperties(cimConductor, rd, importHelper, report);
				if (cimConductor.LengthHasValue)
				{
					rd.AddProperty(new Property(ModelCode.CONDUCTOR_LENGTH, cimConductor.Length));
				}
			}
		}

		public static void PopulateACLineSegmentProperties(FTN.ACLineSegment cimACLineSegment, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimACLineSegment != null) && (rd != null))
			{
				LoadFlowConverter.PopulateConductorProperties(cimACLineSegment, rd, importHelper, report);

				if (cimACLineSegment.XHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_X, cimACLineSegment.X));
				}
				if (cimACLineSegment.X0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_X0, cimACLineSegment.X0));
				}
				if (cimACLineSegment.RHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_R, cimACLineSegment.R));
				}
				if (cimACLineSegment.R0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_R0, cimACLineSegment.R0));
				}
				if (cimACLineSegment.BchHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_BCH, cimACLineSegment.Bch));
				}
				if (cimACLineSegment.B0chHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_B0CH, cimACLineSegment.B0ch));
				}
				if (cimACLineSegment.GchHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_GCH, cimACLineSegment.Gch));
				}
				if (cimACLineSegment.G0chHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ACLINESEGMENT_G0CH, cimACLineSegment.G0ch));
				}
			}
		}

		public static void PopulateEnergyConsumerProperties(FTN.EnergyConsumer cimEnergyConsumer, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimEnergyConsumer != null) && (rd != null))
			{
				LoadFlowConverter.PopulateConductingEquipmentProperties(cimEnergyConsumer, rd, importHelper, report);

				if (cimEnergyConsumer.GroundedHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_GROUNDED, (short)GetDMSWindingConnection(cimEnergyConsumer.Grounded)));
				}
				if (cimEnergyConsumer.PhaseConnectionHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_PHASECONNECTION, (short)GetPhaseShuntConnectionKind(cimEnergyConsumer.PhaseConnection)));
				}
				if (cimEnergyConsumer.PfixedHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_PFIXED, cimEnergyConsumer.Pfixed));
				}
				if (cimEnergyConsumer.PfixedPctHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_PFIXEDPCT, cimEnergyConsumer.PfixedPct));
				}
				if (cimEnergyConsumer.QfixedHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_QFIXED, cimEnergyConsumer.Qfixed));
				}
				if (cimEnergyConsumer.QfixedPctHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYCONSUMER_QFIXEDPCT, cimEnergyConsumer.QfixedPct));
				}
			}
		}

		public static void PopulateEnergySourceProperties(FTN.EnergySource cimEnergySource, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimEnergySource != null) && (rd != null))
			{
				LoadFlowConverter.PopulateConductingEquipmentProperties(cimEnergySource, rd, importHelper, report);

				if (cimEnergySource.RHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_R, cimEnergySource.R));
				}
				if (cimEnergySource.R0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_R0, cimEnergySource.R0));
				}
				if (cimEnergySource.RnHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_RN, cimEnergySource.Rn));
				}
				if (cimEnergySource.XHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_X, cimEnergySource.X));
				}
				if (cimEnergySource.X0HasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_X0, cimEnergySource.X0));
				}
				if (cimEnergySource.XnHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_XN, cimEnergySource.Xn));
				}
				if (cimEnergySource.ActivePowerHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_ACTIVEPOWER, cimEnergySource.ActivePower));
				}
				if (cimEnergySource.NominalVoltageHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_NOMINALVOLTAGE, cimEnergySource.NominalVoltage));
				}
				if (cimEnergySource.VoltageAngleHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_VOLTAGEANGLE, cimEnergySource.VoltageAngle));
				}
				if (cimEnergySource.VoltageMagnitudeHasValue)
				{
					rd.AddProperty(new Property(ModelCode.ENERGYSOURCE_VOLTAGEMAGNITUDE, cimEnergySource.VoltageMagnitude));
				}
			}
		}

		public static void PopulateSwitchProperties(FTN.Switch cimSwitch, ResourceDescription rd, ImportHelper importHelper, TransformAndLoadReport report)
		{
			if ((cimSwitch != null) && (rd != null))
			{
				LoadFlowConverter.PopulateConductingEquipmentProperties(cimSwitch, rd, importHelper, report);

				if (cimSwitch.NormalOpenHasValue)
				{
					rd.AddProperty(new Property(ModelCode.SWITCH_NORMALOPEN, cimSwitch.NormalOpen));
				}
			}
		}


		#endregion

		#region Enums convert

		public static PhaseCode GetDMSPhaseCode(FTN.PhaseCode phases)
		{
			switch (phases)
			{
				case FTN.PhaseCode.A:
					return PhaseCode.A;
				case FTN.PhaseCode.AB:
					return PhaseCode.AB;
				case FTN.PhaseCode.ABC:
					return PhaseCode.ABC;
				case FTN.PhaseCode.ABCN:
					return PhaseCode.ABCN;
				case FTN.PhaseCode.ABN:
					return PhaseCode.ABN;
				case FTN.PhaseCode.AC:
					return PhaseCode.AC;
				case FTN.PhaseCode.ACN:
					return PhaseCode.ACN;
				case FTN.PhaseCode.AN:
					return PhaseCode.AN;
				case FTN.PhaseCode.B:
					return PhaseCode.B;
				case FTN.PhaseCode.BC:
					return PhaseCode.BC;
				case FTN.PhaseCode.BCN:
					return PhaseCode.BCN;
				case FTN.PhaseCode.BN:
					return PhaseCode.BN;
				case FTN.PhaseCode.C:
					return PhaseCode.C;
				case FTN.PhaseCode.CN:
					return PhaseCode.CN;
				case FTN.PhaseCode.N:
					return PhaseCode.N;
				case FTN.PhaseCode.s12N:
					return PhaseCode.ABN;
				case FTN.PhaseCode.s1N:
					return PhaseCode.AN;
				case FTN.PhaseCode.s2N:
					return PhaseCode.BN;
				default: return PhaseCode.Unknown;
			}
		}

		public static WindingConnection GetDMSWindingConnection(FTN.WindingConnection windingConnection)
		{
			switch (windingConnection)
			{
				case FTN.WindingConnection.D:
					return WindingConnection.D;
				case FTN.WindingConnection.I:
					return WindingConnection.I;
				case FTN.WindingConnection.Z:
					return WindingConnection.Z;
				case FTN.WindingConnection.Y:
					return WindingConnection.Y;
				default:
					return WindingConnection.Y;
			}
		}

		public static PhaseShuntConnectionKind GetPhaseShuntConnectionKind(FTN.PhaseShuntConnectionKind phaseShuntConnectionKind)
		{
			switch (phaseShuntConnectionKind)
			{
				case FTN.PhaseShuntConnectionKind.D:
					return PhaseShuntConnectionKind.D;
				case FTN.PhaseShuntConnectionKind.I:
					return PhaseShuntConnectionKind.I;
				case FTN.PhaseShuntConnectionKind.Y:
					return PhaseShuntConnectionKind.Y;
				case FTN.PhaseShuntConnectionKind.Yn:
					return PhaseShuntConnectionKind.Yn;
				default:
					return PhaseShuntConnectionKind.Y;
			}
		}


		#endregion
	}
}
