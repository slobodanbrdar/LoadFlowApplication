using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using NMSGdaImplementation.DataModel.Core;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class EnergySource : ConductingEquipment
	{
		#region Fields
		private float r;

		private float r0;

		private float rn;

		private float x;

		private float x0;

		private float xn;

		private float activaPower;

		private float nominalVoltage;

		private float voltageAngle;

		private float voltageMagnitude;
		#endregion
		public EnergySource(long globalId)
			: base(globalId)
		{
		}

		#region Properties
		public float R
		{
			get { return r; }
			set { r = value; }
		}

		public float R0
		{
			get { return r0; }
			set { r0 = value; }
		}
		public float Rn
		{
			get { return rn; }
			set { rn = value; }
		}

		public float X
		{
			get { return x; }
			set { x = value; }
		}

		public float X0
		{
			get { return x0; }
			set { x0 = value; }
		}

		public float Xn
		{
			get { return xn; }
			set { xn = value; }
		}

		public float ActivePower
		{
			get { return activaPower; }
			set { activaPower = value; }
		}

		public float NominalVoltage
		{
			get { return nominalVoltage; }
			set { nominalVoltage = value; }
		}

		public float VoltageAngle
		{
			get { return voltageAngle; }
			set { voltageAngle = value; }
		}

		public float VoltageMagnitude
		{
			get { return voltageMagnitude; }
			set { voltageMagnitude = value; }
		}
		#endregion

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				EnergySource es = (EnergySource)obj;
				return (es.r == this.r &&
						es.r0 == this.r0 &&
						es.rn == this.rn &&
						es.x == this.x &&
						es.x0 == this.x0 &&
						es.xn == this.xn &&
						es.activaPower == this.activaPower &&
						es.nominalVoltage == this.nominalVoltage &&
						es.voltageAngle == this.voltageAngle &&
						es.VoltageMagnitude == this.VoltageMagnitude);
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
				case ModelCode.ENERGYSOURCE_R:
				case ModelCode.ENERGYSOURCE_R0:
				case ModelCode.ENERGYSOURCE_RN:
				case ModelCode.ENERGYSOURCE_X:
				case ModelCode.ENERGYSOURCE_X0:
				case ModelCode.ENERGYSOURCE_XN:
				case ModelCode.ENERGYSOURCE_ACTIVEPOWER:
				case ModelCode.ENERGYSOURCE_NOMINALVOLTAGE:
				case ModelCode.ENERGYSOURCE_VOLTAGEANGLE:
				case ModelCode.ENERGYSOURCE_VOLTAGEMAGNITUDE:
					return true;
				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.ENERGYSOURCE_R:
					prop.SetValue(r);
					break;
				case ModelCode.ENERGYSOURCE_R0:
					prop.SetValue(r0);
					break;
				case ModelCode.ENERGYSOURCE_RN:
					prop.SetValue(rn);
					break;
				case ModelCode.ENERGYSOURCE_X:
					prop.SetValue(x);
					break;
				case ModelCode.ENERGYSOURCE_X0:
					prop.SetValue(x0);
					break;
				case ModelCode.ENERGYSOURCE_XN:
					prop.SetValue(xn);
					break;
				case ModelCode.ENERGYSOURCE_ACTIVEPOWER:
					prop.SetValue(activaPower);
					break;
				case ModelCode.ENERGYSOURCE_NOMINALVOLTAGE:
					prop.SetValue(nominalVoltage);
					break;
				case ModelCode.ENERGYSOURCE_VOLTAGEANGLE:
					prop.SetValue(voltageAngle);
					break;
				case ModelCode.ENERGYSOURCE_VOLTAGEMAGNITUDE:
					prop.SetValue(voltageMagnitude);
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
				case ModelCode.ENERGYSOURCE_R:
					r = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_R0:
					r0 = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_RN:
					rn = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_X:
					x = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_X0:
					x0 = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_XN:
					xn = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_ACTIVEPOWER:
					ActivePower = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_NOMINALVOLTAGE:
					nominalVoltage = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_VOLTAGEANGLE:
					voltageAngle = property.AsFloat();
					break;
				case ModelCode.ENERGYSOURCE_VOLTAGEMAGNITUDE:
					voltageMagnitude = property.AsFloat();
					break;
				default:
					base.SetProperty(property);
					break;
			}

		}
		#endregion
	}
}
