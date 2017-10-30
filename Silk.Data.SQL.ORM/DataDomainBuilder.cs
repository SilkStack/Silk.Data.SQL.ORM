using Silk.Data.Modelling;
using Silk.Data.SQL.ORM.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM
{
	/// <summary>
	/// Builder API to help construct functioning DataDomain instances.
	/// </summary>
	public class DataDomainBuilder
	{
		private readonly List<DataModelBuilder> _dataModelBuilders = new List<DataModelBuilder>();

		public void AddDataModel<TSource>(Action<EntityModel<TSource>> builtDelegate = null)
			where TSource : new()
		{
			_dataModelBuilders.Add(new DataModelBuilder<TSource>(builtDelegate));
		}

		public void AddDataModel<TSource, TView>(Action<EntityModel<TSource, TView>> builtDelegate = null)
			where TSource : new()
			where TView : new()
		{
			_dataModelBuilders.Add(new DataModelBuilder<TSource, TView>(builtDelegate));
		}

		public DataDomain Build()
		{
			var domain = new DataDomain();

			//  add knowledge of how complex types are mapped/loaded etc.
			foreach (var dataModelBuilder in _dataModelBuilders)
			{
				var viewDefinition = dataModelBuilder.BuildViewDefinition(domain);
				var entityTable = viewDefinition.UserData.OfType<TableDefinition>()
					.FirstOrDefault(q => q.IsEntityTable);
				if (entityTable != null)
				{
					domain.DeclareSchema(dataModelBuilder.ModelType, entityTable);
				}
			}

			//  now make data models
			foreach (var dataModelBuilder in _dataModelBuilders)
			{
				dataModelBuilder.BuildDataModel(domain);
			}

			//  add all the unique schemas
			foreach (var tableSchema in domain.DataModels
				.SelectMany(model => model.Tables).GroupBy(table => table)
				.Select(tableGroup => tableGroup.First()))
			{
				domain.AddSchema(tableSchema);
			}

			return domain;
		}

		private abstract class DataModelBuilder
		{
			public abstract Type ModelType { get; }
			public abstract void BuildDataModel(DataDomain domain);
			public abstract ViewDefinition BuildViewDefinition(DataDomain domain);
		}

		private class DataModelBuilder<TSource> : DataModelBuilder
			where TSource : new()
		{
			private readonly Action<EntityModel<TSource>> _builtDelegate;

			public DataModelBuilder(Action<EntityModel<TSource>> builtDelegate)
			{
				_builtDelegate = builtDelegate;
			}

			public override Type ModelType => typeof(TSource);

			public override void BuildDataModel(DataDomain domain)
			{
				var model = domain.CreateDataModel<TSource>();
				domain.AddModel(model);
				_builtDelegate?.Invoke(model);
			}

			public override ViewDefinition BuildViewDefinition(DataDomain domain)
			{
				ViewDefinition viewDefinition = null;
				var model = TypeModeller.GetModelOf<TSource>();
				model.CreateView(createdViewDefinition =>
				{
					viewDefinition = createdViewDefinition;
					return (EntityModel)null;
				}, new object[] { domain }, domain.ViewConventions);
				return viewDefinition;
			}
		}

		private class DataModelBuilder<TSource,TView> : DataModelBuilder
			where TSource : new()
			where TView : new()
		{
			public override Type ModelType => typeof(TSource);

			private readonly Action<EntityModel<TSource, TView>> _builtDelegate;

			public DataModelBuilder(Action<EntityModel<TSource, TView>> builtDelegate)
			{
				_builtDelegate = builtDelegate;
			}

			public override void BuildDataModel(DataDomain domain)
			{
				var model = domain.CreateDataModel<TSource, TView>();
				domain.AddModel(model);
				_builtDelegate?.Invoke(model);
			}

			public override ViewDefinition BuildViewDefinition(DataDomain domain)
			{
				ViewDefinition viewDefinition = null;
				var model = TypeModeller.GetModelOf<TSource>();
				model.CreateView(createdViewDefinition =>
				{
					viewDefinition = createdViewDefinition;
					return (EntityModel)null;
				}, typeof(TView), new object[] { domain }, domain.ViewConventions);
				return viewDefinition;
			}
		}
	}
}
