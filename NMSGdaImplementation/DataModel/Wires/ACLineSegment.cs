using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class ACLineSegment : Conductor
	{
		#region Fields
		private float x;

		private float x0;

		private float r;

		private float r0;

		private float bch;

		private float b0ch;

		private float gch;

		private float g0ch;
		#endregion
		public ACLineSegment(long globalId)
			: base(globalId)
		{
		}

		#region Properties
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

		public float Bch
		{
			get { return bch; }
			set { bch = value; }
		}

		public float B0ch
		{
			get { return b0ch; }
			set { b0ch = value; }
		}

		public float Gch
		{
			get { return gch; }
			set { gch = value; }
		}

		public float G0ch
		{
			get { return g0ch; }
			set { g0ch = value; }
		}
		#endregion

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				ACLineSegment acls = (ACLineSegment)obj;
				return (acls.x == this.x &&
						acls.x0 == this.x0 &&
						acls.r == this.r &&
						acls.r0 == this.r0 &&
						acls.bch == this.bch &&
						acls.b0ch == this.b0ch &&
						acls.gch == this.g0ch &&
						acls.g0ch == this.g0ch);
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
				case ModelCode.ACLINESEGMENT_BCH:
				case ModelCode.ACLINESEGMENT_B0CH:
				case ModelCode.ACLINESEGMENT_GCH:
				case ModelCode.ACLINESEGMENT_G0CH:
				case ModelCode.ACLINESEGMENT_R:
				case ModelCode.ACLINESEGMENT_R0:
				case ModelCode.ACLINESEGMENT_X:
				case ModelCode.ACLINESEGMENT_X0:
					return true;
				default:
					return base.HasProperty(property);
			}

		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.ACLINESEGMENT_BCH:
					prop.SetValue(bch);
					break;
				case ModelCode.ACLINESEGMENT_B0CH:
					prop.SetValue(b0ch);
					break;
				case ModelCode.ACLINESEGMENT_GCH:
					prop.SetValue(gch);
					break;
				case ModelCode.ACLINESEGMENT_G0CH:
					prop.SetValue(g0ch);
					break;
				case ModelCode.ACLINESEGMENT_R:
					prop.SetValue(r);
					break;
				case ModelCode.ACLINESEGMENT_R0:
					prop.SetValue(r0);
					break;
				case ModelCode.ACLINESEGMENT_X:
					prop.SetValue(x);
					break;
				case ModelCode.ACLINESEGMENT_X0:
					prop.SetValue(x0);
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
				case ModelCode.ACLINESEGMENT_BCH:
					bch = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_B0CH:
					b0ch = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_GCH:
					gch = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_G0CH:
					g0ch = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_R:
					r = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_R0:
					r0 = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_X:
					x = property.AsFloat();
					break;
				case ModelCode.ACLINESEGMENT_X0:
					x0 = property.AsFloat();
					break;
				default:
					base.SetProperty(property);
					break;
			}
		}
		#endregion
	}
}
