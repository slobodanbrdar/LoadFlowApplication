using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using System.Collections.Generic;

namespace NMSGdaImplementation.DataModel.Core
{
	public class Terminal : IdentifiedObject
	{
		#region Fields
		private PhaseCode phases;

		private long conductingEquipment = 0;

		private long connectivityNode = 0;
		#endregion


		public Terminal(long globalId)
			: base(globalId)
		{
		}

		#region Properties
		public PhaseCode Phases
		{
			get { return phases; }
			set { phases = value; }
		}

		public long ConductingEquipment
		{
			get { return conductingEquipment; }
			set { conductingEquipment = value; }
		}

		public long ConncetivityNode
		{
			get { return connectivityNode; }
			set { connectivityNode = value; }
		}
		#endregion

		public override bool Equals(object x)
		{
			if (base.Equals(x))
			{
				Terminal t = (Terminal)x;
				return (t.ConncetivityNode == this.ConncetivityNode &&
						t.ConductingEquipment == this.ConductingEquipment &&
						t.Phases == this.Phases);
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
				case ModelCode.TERMINAL_PHASES:
				case ModelCode.TERMINAL_CONDEQ:
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					return true;
				default:
					return base.HasProperty(property);
			}

		}

		public override void GetProperty(Property property)
		{
			switch (property.Id)
			{
				case ModelCode.TERMINAL_PHASES:
					property.SetValue((short)phases);
					break;
				case ModelCode.TERMINAL_CONDEQ:
					property.SetValue(conductingEquipment);
					break;
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					property.SetValue(connectivityNode);
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
				case ModelCode.TERMINAL_PHASES:
					phases = (PhaseCode)property.AsEnum();
					break;
				case ModelCode.TERMINAL_CONDEQ:
					conductingEquipment = property.AsReference();
					break;
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					connectivityNode = property.AsReference();
					break;
				default:
					base.SetProperty(property);
					break;
			}

		}
		#endregion

		#region IReference implementation
		public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			if (conductingEquipment != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
			{
				references[ModelCode.TERMINAL_CONDEQ] = new List<long>();
				references[ModelCode.TERMINAL_CONDEQ].Add(conductingEquipment);
			}
			if (connectivityNode != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
			{
				references[ModelCode.TERMINAL_CONNECTIVITYNODE] = new List<long>();
				references[ModelCode.TERMINAL_CONNECTIVITYNODE].Add(connectivityNode);
			}
			base.GetReferences(references, refType);
		}
		#endregion

	}
}
