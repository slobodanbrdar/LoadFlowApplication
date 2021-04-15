using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using NMSGdaImplementation.DataModel.Core;
using System.Collections.Generic;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class PowerTransformerEnd : IdentifiedObject
	{
		#region Fields
		private WindingConnection connectionKind;

		private float b;

		private float b0;

		private float g;

		private float g0;

		private float r;

		private float r0;

		private float x;

		private float x0;

		private float ratedS;

		private float ratedU;

		private int endNumber;

		private long powerTransformer = 0;
		#endregion


		public PowerTransformerEnd(long globalId)
			: base(globalId)
		{

		}

		#region Properties
		public WindingConnection ConnectionKind
		{
			get { return connectionKind; }
			set { connectionKind = value; }
		}

		public float B
		{
			get { return b; }
			set { b = value; }
		}

		public float B0
		{
			get { return b0; }
			set { b0 = value; }
		}

		public float G
		{
			get { return g; }
			set { g = value; }
		}

		public float G0
		{
			get { return g0; }
			set { g0 = value; }
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

		public float RatedS
		{
			get { return ratedS; }
			set { ratedS = value; }
		}

		public float RatedU
		{
			get { return ratedU; }
			set { ratedU = value; }
		}

		public int EndNumber
		{
			get { return endNumber; }
			set { endNumber = value; }
		}

		public long PowerTransformer
		{
			get { return powerTransformer; }
			set { powerTransformer = value; }
		}
		#endregion

		public override bool Equals(object x)
		{
			if (base.Equals(x))
			{
				PowerTransformerEnd pte = (PowerTransformerEnd)x;
				return (pte.B == this.B &&
						pte.B0 == this.B0 &&
						pte.G == this.G &&
						pte.G0 == this.G0 &&
						pte.R == this.R &&
						pte.R0 == this.R0 &&
						pte.X == this.X &&
						pte.X0 == this.X0 &&
						pte.RatedS == this.RatedS &&
						pte.RatedU == this.RatedU &&
						pte.EndNumber == this.EndNumber &&
						pte.PowerTransformer == this.PowerTransformer &&
						pte.ConnectionKind == this.ConnectionKind
					);
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

				case ModelCode.PTRANSFORMEREND_CONNKIND:
				case ModelCode.PTRANSFORMEREND_B:
				case ModelCode.PTRANSFORMEREND_B0:
				case ModelCode.PTRANSFORMEREND_G:
				case ModelCode.PTRANSFORMEREND_G0:
				case ModelCode.PTRANSFORMEREND_R:
				case ModelCode.PTRANSFORMEREND_R0:
				case ModelCode.PTRANSFORMEREND_X:
				case ModelCode.PTRANSFORMEREND_X0:
				case ModelCode.PTRANSFORMEREND_RATEDS:
				case ModelCode.PTRANSFORMEREND_RATEDU:
				case ModelCode.PTRANSFORMEREND_POWERTRANSFORMER:
				case ModelCode.PTRANSFORMEREND_ENDNUMBER:
					return true;
				default:
					return base.HasProperty(property);

			}
		}

		public override void GetProperty(Property property)
		{
			switch (property.Id)
			{
				case ModelCode.PTRANSFORMEREND_CONNKIND:
					property.SetValue((short)connectionKind);
					break;
				case ModelCode.PTRANSFORMEREND_B:
					property.SetValue(b);
					break;
				case ModelCode.PTRANSFORMEREND_B0:
					property.SetValue(b0);
					break;
				case ModelCode.PTRANSFORMEREND_G:
					property.SetValue(g);
					break;
				case ModelCode.PTRANSFORMEREND_G0:
					property.SetValue(g0);
					break;
				case ModelCode.PTRANSFORMEREND_R:
					property.SetValue(r);
					break;
				case ModelCode.PTRANSFORMEREND_R0:
					property.SetValue(r0);
					break;
				case ModelCode.PTRANSFORMEREND_X:
					property.SetValue(x);
					break;
				case ModelCode.PTRANSFORMEREND_X0:
					property.SetValue(X0);
					break;
				case ModelCode.PTRANSFORMEREND_RATEDS:
					property.SetValue(ratedS);
					break;
				case ModelCode.PTRANSFORMEREND_RATEDU:
					property.SetValue(ratedU);
					break;
				case ModelCode.PTRANSFORMEREND_POWERTRANSFORMER:
					property.SetValue(powerTransformer);
					break;
				case ModelCode.PTRANSFORMEREND_ENDNUMBER:
					property.SetValue(endNumber);
					break;
				default:
					base.GetProperty(property);
					break;
			}
		}

		public override void SetProperty(Property property)
		{
			switch (property.Id)
			{

				case ModelCode.PTRANSFORMEREND_CONNKIND:
					connectionKind = (WindingConnection)property.AsEnum();
					break;
				case ModelCode.PTRANSFORMEREND_B:
					b = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_B0:
					b0 = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_G:
					g = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_G0:
					g0 = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_R:
					r = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_R0:
					r0 = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_X:
					x = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_X0:
					X0 = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_RATEDS:
					ratedS = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_RATEDU:
					ratedU = property.AsFloat();
					break;
				case ModelCode.PTRANSFORMEREND_POWERTRANSFORMER:
					powerTransformer = property.AsReference();
					break;
				case ModelCode.PTRANSFORMEREND_ENDNUMBER:
					endNumber = property.AsInt();
					break;
				default:
					base.SetProperty(property);
					break;
			}
		}
		#endregion

		#region IReference implemenataion
		public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			if (powerTransformer != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
			{
				references[ModelCode.PTRANSFORMEREND_POWERTRANSFORMER] = new List<long>();
				references[ModelCode.PTRANSFORMEREND_POWERTRANSFORMER].Add(powerTransformer);
			}
			base.GetReferences(references, refType);
		}
		#endregion

	}
}
