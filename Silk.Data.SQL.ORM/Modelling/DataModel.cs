using Silk.Data.Modelling;
using Silk.Data.Modelling.ResourceLoaders;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public abstract class DataModel : IView<DataField>
	{
		public DataField[] Fields { get; }

		public string Name { get; }

		public Model Model { get; }

		public IResourceLoader[] ResourceLoaders { get; }

		IViewField[] IView.Fields => Fields;

		public TableSchema[] Tables { get; }

		public DataModel(string name, Model model, DataField[] fields,
			IResourceLoader[] resourceLoaders)
		{
			Name = name;
			Model = model;
			Fields = fields;
			ResourceLoaders = resourceLoaders;
			Tables = Fields.Select(q => q.Storage.Table).GroupBy(q => q)
				.Select(q => q.First()).ToArray();
		}
	}

	public class DataModel<TSource, TView> : DataModel, IView<DataField, TSource, TView>
		where TSource : new()
		where TView: new()
	{
		public new TypedModel<TSource> Model { get; }

		public DataModel(string name, TypedModel<TSource> model, DataField[] fields, IResourceLoader[] resourceLoaders)
			: base(name, model, fields, resourceLoaders)
		{
			Model = model;
		}

		public void Insert(IDataProvider dataProvider, params TSource[] sources)
		{
			//  todo: update this to work with datamodels that span multiple tables
			var table = Fields.First().Storage.Table;
			var queries = new List<QueryExpression>();
			var columns = Fields.Where(
				dataField => !dataField.Storage.IsAutoIncrement
				).ToArray();
			var values = new List<QueryExpression[]>();

			GenerateIds(sources);

			var views = this.MapToViewAsync(sources).ConfigureAwait(false)
				.GetAwaiter().GetResult();

			var viewContainer = new ObjectContainer<TView>(Model, this);
			foreach (var view in views)
			{
				viewContainer.Instance = view;
				var row = new QueryExpression[columns.Length];
				for(var i = 0; i < columns.Length; i++)
				{
					row[i] = QueryExpression.Value(viewContainer.GetValue(
						columns[i].ModelBinding.ViewFieldPath
						));
				}
				values.Add(row);
			}

			queries.Add(QueryExpression.Insert(table.TableName,
				columns.Select(
					dataField => dataField.Name
				).ToArray(),
				values.ToArray()
				));

			dataProvider.ExecuteNonQuery(QueryExpression.Transaction(queries));
		}

		private void GenerateIds(TSource[] sources)
		{
			var autoGenerateField = Fields.FirstOrDefault(q => q.Storage.IsAutoGenerate &&
				q.DataType == typeof(Guid));

			if (autoGenerateField != null)
			{
				var autoGenerateModelField = Model.GetField(autoGenerateField.ModelBinding.ModelFieldPath);
				var modelWriter = new ObjectReadWriter(typeof(TSource), Model, null);

				foreach (var source in sources)
				{
					modelWriter.Value = source;
					modelWriter.GetField(autoGenerateModelField).Value = Guid.NewGuid();
				}
			}
		}
	}
}
