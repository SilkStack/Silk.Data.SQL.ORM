using System;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class DataRelationship
    {
		private readonly Lazy<DataField> _lazyForeignField;
		private readonly Lazy<DataModel> _lazyForeignModel;

		public DataField ForeignField => _lazyForeignField.Value;
		public DataModel ForeignModel => _lazyForeignModel.Value;

		public DataRelationship(Lazy<DataField> lazyForeignField, Lazy<DataModel> lazyForeignModel)
		{
			_lazyForeignField = lazyForeignField;
			_lazyForeignModel = lazyForeignModel;
		}
    }
}
