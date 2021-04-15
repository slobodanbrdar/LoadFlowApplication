using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using System.Collections.Generic;

namespace NMSGdaImplementation.DataModel.Core
{
	public class BaseVoltage : IdentifiedObject
	{

		private float nominalVoltage;

		private List<long> conductingEquipments = new List<long>();

		public BaseVoltage(long globalId) : base(globalId)
		{
		}

		public float NominalVoltage
		{
			get
			{
				return nominalVoltage;
			}

			set
			{
				nominalVoltage = value;
			}
		}

		public List<long> ConductingEquipments
		{
			get
			{
				return conductingEquipments;
			}

			set
			{
				conductingEquipments = value;
			}
		}

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				BaseVoltage x = (BaseVoltage)obj;
				return ((x.NominalVoltage == this.NominalVoltage) &&
						(CompareHelper.CompareLists(x.conductingEquipments, this.conductingEquipments)));
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
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
				case ModelCode.BASEVOLTAGE_CONDEQS:
					return true;

				default:
					return base.HasProperty(t);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
					prop.SetValue(nominalVoltage);
					break;
				case ModelCode.BASEVOLTAGE_CONDEQS:
					prop.SetValue(conductingEquipments);
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
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
					nominalVoltage = property.AsFloat();
					break;

				default:
					base.SetProperty(property);
					break;
			}
		}

		#endregion IAccess implementation	

		#region IReference implementation

		public override bool IsReferenced
		{
			get
			{
				return conductingEquipments.Count > 0 || base.IsReferenced;
			}
		}

		public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			if (conductingEquipments != null && conductingEquipments.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
			{
				references[ModelCode.BASEVOLTAGE_CONDEQS] = conductingEquipments.GetRange(0, conductingEquipments.Count);
			}

			base.GetReferences(references, refType);
		}

		public override void AddReference(ModelCode referenceId, long globalId)
		{
			switch (referenceId)
			{
				case ModelCode.CONDEQ_BASVOLTAGE:
					conductingEquipments.Add(globalId);
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
				case ModelCode.CONDEQ_BASVOLTAGE:

					if (conductingEquipments.Contains(globalId))
					{
						conductingEquipments.Remove(globalId);
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
