using Common.NetworkModelService;
using Common.NetworkModelService.GenericDataAccess;

namespace NMSGdaImplementation.DataModel.Core
{
	public class Equipment : PowerSystemResource
	{


		public Equipment(long globalId) : base(globalId)
		{
		}


		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IAccess implementation

		public override bool HasProperty(ModelCode property)
		{
			return base.HasProperty(property);
		}

		public override void GetProperty(Property property)
		{
			base.GetProperty(property);
		}

		public override void SetProperty(Property property)
		{
			base.SetProperty(property);
		}

		#endregion IAccess implementation
	}
}
