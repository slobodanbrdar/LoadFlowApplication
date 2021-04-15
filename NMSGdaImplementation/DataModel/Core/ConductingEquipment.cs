using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using System.Collections.Generic;

namespace NMSGdaImplementation.DataModel.Core
{
	public class ConductingEquipment : Equipment
	{

		private long baseVoltage = 0;
		private List<long> terminals = new List<long>();

		public ConductingEquipment(long globalId) : base(globalId)
		{
		}

		public long BaseVoltage
		{
			get { return baseVoltage; }
			set { baseVoltage = value; }
		}
		public List<long> Terminals
		{
			get { return terminals; }
			set { terminals = value; }
		}

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				ConductingEquipment x = (ConductingEquipment)obj;
				return (x.baseVoltage == this.baseVoltage &&
						CompareHelper.CompareLists(x.terminals, this.terminals));
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
				case ModelCode.CONDEQ_BASVOLTAGE:
				case ModelCode.CONDEQ_TERMINALS:
					return true;

				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.CONDEQ_BASVOLTAGE:
					prop.SetValue(baseVoltage);
					break;
				case ModelCode.CONDEQ_TERMINALS:
					prop.SetValue(terminals);
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
				case ModelCode.CONDEQ_BASVOLTAGE:
					baseVoltage = property.AsReference();
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
				return terminals.Count > 0 || base.IsReferenced;
			}
		}

		public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			if (baseVoltage != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
			{
				references[ModelCode.CONDEQ_BASVOLTAGE] = new List<long>();
				references[ModelCode.CONDEQ_BASVOLTAGE].Add(baseVoltage);
			}
			if (terminals != null && terminals.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
			{
				references[ModelCode.CONDEQ_TERMINALS] = terminals.GetRange(0, terminals.Count);
			}

			base.GetReferences(references, refType);
		}

		public override void AddReference(ModelCode referenceId, long globalId)
		{
			switch (referenceId)
			{
				case ModelCode.TERMINAL_CONDEQ:
					terminals.Add(globalId);
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
				case ModelCode.TERMINAL_CONDEQ:
					if (terminals.Contains(globalId))
					{
						terminals.Remove(globalId);
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
