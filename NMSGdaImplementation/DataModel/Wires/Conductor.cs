using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using NMSGdaImplementation.DataModel.Core;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class Conductor : ConductingEquipment
	{
		private float length;

		public Conductor(long globalId) : base(globalId)
		{
		}

		public float Length
		{
			get { return length; }
			set { length = value; }
		}

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				Conductor c = (Conductor)obj;
				return c.length == this.length;
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
				case ModelCode.CONDUCTOR_LENGTH:
					return true;
				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.CONDUCTOR_LENGTH:
					prop.SetValue(length);
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
				case ModelCode.CONDUCTOR_LENGTH:
					length = property.AsFloat();
					break;
				default:
					base.SetProperty(property);
					break;
			}
		}
		#endregion
	}
}
