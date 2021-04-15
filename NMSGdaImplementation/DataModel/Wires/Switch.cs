using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using NMSGdaImplementation.DataModel.Core;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class Switch : ConductingEquipment
	{
		private bool normalOpen;



		public Switch(long globalId)
			: base(globalId)
		{
		}

		public bool NormalOpen
		{
			get { return normalOpen; }
			set { normalOpen = value; }
		}

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				Switch s = (Switch)obj;
				return s.normalOpen == this.normalOpen;
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
				case ModelCode.SWITCH_NORMALOPEN:
					return true;
				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.SWITCH_NORMALOPEN:
					prop.SetValue(normalOpen);
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
				case ModelCode.SWITCH_NORMALOPEN:
					normalOpen = property.AsBool();
					break;
				default:
					base.SetProperty(property);
					break;
			}
		}
		#endregion
	}
}
