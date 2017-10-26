using Silk.Data.Modelling;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using Silk.Data.SQL.ORM.Modelling.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Describes the types, models and resource loaders of a data domain.
	/// </summary>
	public class DataDomain
	{
		private static ViewConvention[] _defaultViewConventions = new ViewConvention[]
		{
			new CleanModelNameConvention(),
			new CopySupportedSQLTypesConvention(),
			new IdIsPrimaryKeyConvention(),
			new CopyPrimaryKeyOfTypesWithSchemaConvention()
		};

		private readonly List<DataModel> _dataModels = new List<DataModel>();
		private readonly List<TableSchema> _tables = new List<TableSchema>();
		private readonly Dictionary<Type, TableDefinition> _entitySchemas = new Dictionary<Type, TableDefinition>();

		public ViewConvention[] ViewConventions { get; }
		public IReadOnlyCollection<DataModel> DataModels => _dataModels;
		public IReadOnlyCollection<TableSchema> Tables => _tables;

		public DataDomain() :
			this(_defaultViewConventions)
		{
		}

		public DataDomain(ViewConvention[] viewConventions)
		{
			ViewConventions = viewConventions;
		}

		public void DeclareSchema(Type type, TableDefinition tableDefinition)
		{
			_entitySchemas.Add(type, tableDefinition);
		}

		public TableDefinition GetDeclaredSchema(Type type)
		{
			_entitySchemas.TryGetValue(type, out var tableDefinition);
			return tableDefinition;
		}

		public void AddSchema(TableSchema tableSchema)
		{
			_tables.Add(tableSchema);
		}

		public void AddModel(DataModel dataModel)
		{
			_dataModels.Add(dataModel);
		}

		/// <summary>
		/// Creates a data model using knowledge from the data domain.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <returns></returns>
		public DataModel<TSource> CreateDataModel<TSource>()
			where TSource : new()
		{
			var model = TypeModeller.GetModelOf<TSource>();
			var dataModel = model.CreateView(viewDefinition =>
			{
				return new DataModel<TSource>(
					viewDefinition.Name, model,
					DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray(), this);
			}, new object[] { this }, ViewConventions);
			return dataModel;
		}

		/// <summary>
		/// Creates a data model using knowledge from the data domain.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <returns></returns>
		public DataModel<TSource, TView> CreateDataModel<TSource, TView>()
			where TSource : new()
			where TView : new()
		{
			var model = TypeModeller.GetModelOf<TSource>();
			var dataModel = model.CreateView(viewDefinition =>
			{
				return new DataModel<TSource, TView>(
					viewDefinition.Name, model,
					DataField.FromDefinitions(viewDefinition.UserData.OfType<TableDefinition>(), viewDefinition.FieldDefinitions).ToArray(),
					viewDefinition.ResourceLoaders.ToArray(), this);
			}, typeof(TView), new object[] { this }, ViewConventions);
			return dataModel;
		}
	}
}
