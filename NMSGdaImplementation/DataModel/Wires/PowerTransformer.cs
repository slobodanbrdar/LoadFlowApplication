using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using NMSGdaImplementation.DataModel.Core;
using System.Collections.Generic;

namespace NMSGdaImplementation.DataModel.Wires
{
	public class PowerTransformer : ConductingEquipment
	{

		private List<long> transformerEnds = new List<long>();

		public PowerTransformer(long globalId)
			: base(globalId)
		{
		}

		public List<long> TransformerEnds
		{
			get { return transformerEnds; }
			set { transformerEnds = value; }
		}

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				PowerTransformer x = (PowerTransformer)obj;
				return (CompareHelper.CompareLists(x.TransformerEnds, this.TransformerEnds, true));
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

		public override bool HasProperty(ModelCode t)
		{
			switch (t)
			{
				case ModelCode.POWERTRANSFORMER_TRANSFORMERENDS:
					return true;

				default:
					return base.HasProperty(t);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{

				case ModelCode.POWERTRANSFORMER_TRANSFORMERENDS:
					prop.SetValue(transformerEnds);
					break;
				default:
					base.GetProperty(prop);
					break;
			}
		}

		public override void SetProperty(Property property)
		{
			base.SetProperty(property);
		}

		#endregion IAccess implementation



		#region IReference implementation

		public override bool IsReferenced
		{
			get
			{
				return (transformerEnds.Count > 0) || base.IsReferenced;
			}
		}

		public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			if (transformerEnds != null && transformerEnds.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
			{
				references[ModelCode.POWERTRANSFORMER_TRANSFORMERENDS] = transformerEnds.GetRange(0, transformerEnds.Count);
			}

			base.GetReferences(references, refType);
		}

		public override void AddReference(ModelCode referenceId, long globalId)
		{
			switch (referenceId)
			{
				case ModelCode.PTRANSFORMEREND_POWERTRANSFORMER:
					transformerEnds.Add(globalId);
					break;

				default:
					base.AddReference(referenceId, globalId);
					break;
			}
		}

		public override void RemoveReference(ModelCode referenceId, long globalId)
		{
			switch (referenceId)
			{
				case ModelCode.PTRANSFORMEREND_POWERTRANSFORMER:

					if (transformerEnds.Contains(globalId))
					{
						transformerEnds.Remove(globalId);
					}
					else
					{
						Logger.LogWarning($"Entity (GID = 0x{this.GlobalId:x16}) doesn't contain reference 0x{globalId:x16}");
					}

					break;
				default:
					base.RemoveReference(referenceId, globalId);
					break;
			}
		}

		#endregion IReference implementation
	}
}
