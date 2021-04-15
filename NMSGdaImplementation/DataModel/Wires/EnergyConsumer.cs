using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using NMSGdaImplementation.DataModel.Core;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class EnergyConsumer : ConductingEquipment
	{
		#region Fields
		private WindingConnection grounded;

		private PhaseShuntConnectionKind phaseConnection;

		private float pFixed;

		private float pFixedPct;

		private float qFixed;

		private float qFixedPct;
		#endregion
		public EnergyConsumer(long globalId)
			: base(globalId)
		{
		}

		#region Properties
		public WindingConnection Grounded
		{
			get { return grounded; }
			set { grounded = value; }
		}

		public PhaseShuntConnectionKind PhaseConnection
		{
			get { return phaseConnection; }
			set { phaseConnection = value; }
		}

		public float PFixed
		{
			get { return pFixed; }
			set { pFixed = value; }
		}

		public float PFixedPct
		{
			get { return pFixedPct; }
			set { pFixedPct = value; }
		}

		public float QFixed
		{
			get { return qFixed; }
			set { qFixed = value; }
		}

		public float QFixedPct
		{
			get { return qFixedPct; }
			set { qFixedPct = value; }
		}
		#endregion

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				EnergyConsumer ec = (EnergyConsumer)obj;
				return (ec.grounded == this.grounded &&
					ec.phaseConnection == this.phaseConnection &&
					ec.pFixed == this.pFixed &&
					ec.pFixedPct == this.pFixedPct &&
					ec.qFixed == this.qFixed &&
					ec.qFixedPct == this.qFixedPct);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IAccess implementation
		public override bool HasProperty(ModelCode property)
		{
			switch (property)
			{
				case ModelCode.ENERGYCONSUMER_GROUNDED:
				case ModelCode.ENERGYCONSUMER_PHASECONNECTION:
				case ModelCode.ENERGYCONSUMER_PFIXED:
				case ModelCode.ENERGYCONSUMER_PFIXEDPCT:
				case ModelCode.ENERGYCONSUMER_QFIXED:
				case ModelCode.ENERGYCONSUMER_QFIXEDPCT:
					return true;
				default:
					return base.HasProperty(property);
			}

		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.ENERGYCONSUMER_GROUNDED:
					prop.SetValue((short)grounded);
					break;
				case ModelCode.ENERGYCONSUMER_PHASECONNECTION:
					prop.SetValue((short)phaseConnection);
					break;
				case ModelCode.ENERGYCONSUMER_PFIXED:
					prop.SetValue(pFixed);
					break;
				case ModelCode.ENERGYCONSUMER_PFIXEDPCT:
					prop.SetValue(pFixedPct);
					break;
				case ModelCode.ENERGYCONSUMER_QFIXED:
					prop.SetValue(qFixed);
					break;
				case ModelCode.ENERGYCONSUMER_QFIXEDPCT:
					prop.SetValue(qFixedPct);
					break;
				default:
					base.GetProperty(prop);
					break;
			}
		}

		public override void SetProperty(Property property)
		{
			switch (property.Id)
			{
				case ModelCode.ENERGYCONSUMER_GROUNDED:
					grounded = (WindingConnection)property.AsEnum();
					break;
				case ModelCode.ENERGYCONSUMER_PHASECONNECTION:
					phaseConnection = (PhaseShuntConnectionKind)property.AsEnum();
					break;
				case ModelCode.ENERGYCONSUMER_PFIXED:
					pFixed = property.AsFloat();
					break;
				case ModelCode.ENERGYCONSUMER_PFIXEDPCT:
					pFixedPct = property.AsFloat();
					break;
				case ModelCode.ENERGYCONSUMER_QFIXED:
					qFixed = property.AsFloat();
					break;
				case ModelCode.ENERGYCONSUMER_QFIXEDPCT:
					qFixedPct = property.AsFloat();
					break;
				default:
					base.SetProperty(property);
					break;
			}

		}
		#endregion
	}
}
