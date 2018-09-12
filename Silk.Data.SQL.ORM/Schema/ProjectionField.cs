using Silk.Data.Modelling;
using Silk.Data.Modelling.Mapping.Binding;
using Silk.Data.SQL.Expressions;
using Silk.Data.SQL.ORM.Schema.Binding;
using CoreBinding = Silk.Data.Modelling.Mapping.Binding;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Describs how a field is mapped from schema to entity type.
	/// </summary>
	public abstract class ProjectionField : IProjectedItem
	{
		public abstract bool IsNullCheck { get; }

		/// <summary>
		/// Gets the table name/alias that the field is a member of.
		/// </summary>
		public string SourceName { get; }
		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		public string FieldName { get; }
		/// <summary>
		/// Gets the alias the field should be projected as.
		/// </summary>
		public string AliasName { get; }
		/// <summary>
		/// Gets the path to the property on the entity model.
		/// </summary>
		public string[] ModelPath { get; }

		public EntityFieldJoin Join { get; }

		public ProjectionField(string sourceName, string fieldName, string aliasName,
			string[] modelPath, EntityFieldJoin join)
		{
			SourceName = sourceName;
			FieldName = fieldName;
			AliasName = aliasName;
			ModelPath = modelPath;
			Join = join;
		}

		public abstract CoreBinding.Binding GetMappingBinding(string aliasPrefix);

		public AliasExpression GetExpression(string aliasPrefix)
		{
			return QueryExpression.Alias(QueryExpression.Column(FieldName, new AliasIdentifierExpression(SourceName)), $"{aliasPrefix}{AliasName}");
		}
	}

	public class ProjectionField<T> : ProjectionField
	{
		public override bool IsNullCheck { get; }

		public ProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath, EntityFieldJoin join)
			: base(sourceName, fieldName, aliasName, modelPath, join)
		{
			IsNullCheck = !SqlTypeHelper.IsSqlPrimitiveType(typeof(T));
		}

		public override CoreBinding.Binding GetMappingBinding(string aliasPrefix)
		{
			if (IsNullCheck)
				return new CreateInstanceWithNullCheck<T, bool>(
					SqlTypeHelper.GetConstructor(typeof(T)),
					new[] { AliasName },
					ModelPath);

			return new CoreBinding.CopyBinding<T>(new[] { $"{aliasPrefix}{AliasName}" }, ModelPath);
		}
	}

	public class ProjectionField<TEntity, TKeyValue> : ProjectionField
	{
		public override bool IsNullCheck => true;

		public ProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath, EntityFieldJoin join)
			: base(sourceName, fieldName, aliasName, modelPath, join)
		{
		}

		public override CoreBinding.Binding GetMappingBinding(string aliasPrefix)
		{
			return new CreateInstanceWithNullCheck<TEntity, TKeyValue>(
				SqlTypeHelper.GetConstructor(typeof(TEntity)),
				new[] { $"{aliasPrefix}{AliasName}" },
				ModelPath);
		}
	}

	public class MappedProjectionField : ProjectionField
	{
		public override bool IsNullCheck { get; }

		private readonly ProjectionField _sourceProjection;
		private readonly MappingBinding _mappingBinding;

		public MappedProjectionField(string sourceName, string fieldName, string aliasName, string[] modelPath,
			EntityFieldJoin join, bool isNullCheck, ProjectionField sourceProjection, MappingBinding mappingBinding)
			: base(sourceName, fieldName, aliasName, modelPath, join)
		{
			IsNullCheck = isNullCheck;
			_sourceProjection = sourceProjection;
			_mappingBinding = mappingBinding;
		}

		public override CoreBinding.Binding GetMappingBinding(string aliasPrefix)
		{
			var sourceBinding = _sourceProjection.GetMappingBinding(aliasPrefix);
			return new MappedBinding(sourceBinding, _mappingBinding);
		}

		private class MappedBinding : CoreBinding.Binding
		{
			private readonly CoreBinding.Binding _sourceBinding;
			private readonly MappingBinding _mappingBinding;

			public MappedBinding(CoreBinding.Binding sourceBinding, MappingBinding mappingBinding)
			{
				_sourceBinding = sourceBinding;
				_mappingBinding = mappingBinding;
			}

			public override void PerformBinding(IModelReadWriter from, IModelReadWriter to)
			{
				var mappedReadWriter = new MappedReadWriter();
				_sourceBinding.PerformBinding(from, mappedReadWriter);
				_mappingBinding.PerformBinding(mappedReadWriter, to);
			}
		}

		private class MappedReadWriter : IModelReadWriter
		{
			private object _value;

			public IModel Model => throw new System.NotImplementedException();

			public T ReadField<T>(string[] path, int offset)
			{
				return (T)_value;
			}

			public void WriteField<T>(string[] path, int offset, T value)
			{
				_value = value;
			}
		}
	}
}
