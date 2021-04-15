using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;
using System.Collections.Generic;

namespace NMSGdaImplementation.DataModel.Core
{
	public class ConnectivityNode : IdentifiedObject
	{
		private List<long> terminals = new List<long>();

		public ConnectivityNode(long globalId)
			: base(globalId)
		{
		}

		public List<long> Terminals
		{
			get { return terminals; }
			set { terminals = value; }
		}

		public override bool Equals(object x)
		{
			if (base.Equals(x))
			{
				ConnectivityNode cn = (ConnectivityNode)x;
				return CompareHelper.CompareLists(cn.terminals, this.terminals);
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
				case ModelCode.CONNECTIVITYNODE_TERMINALS:

					return true;
				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property property)
		{
			switch (property.Id)
			{
				case ModelCode.CONNECTIVITYNODE_TERMINALS:
					property.SetValue(terminals);
					break;
				default:
					base.GetProperty(property);
					break;
			}
		}
		#endregion

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
			if (terminals != null && terminals.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
			{
				references[ModelCode.CONNECTIVITYNODE_TERMINALS] = terminals.GetRange(0, terminals.Count);
			}
			base.GetReferences(references, refType);
		}

		public override void AddReference(ModelCode referenceId, long globalId)
		{
			switch (referenceId)
			{
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
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
		#endregion
	}
}
