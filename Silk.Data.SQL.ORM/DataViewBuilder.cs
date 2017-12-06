using System;
using Silk.Data.Modelling;
using Silk.Data.Modelling.Bindings;
using Silk.Data.Modelling.Conventions;
using Silk.Data.SQL.ORM.Modelling;
using System.Linq;
using System.Collections.Generic;

namespace Silk.Data.SQL.ORM
{
	public class DataViewBuilder : ViewBuilder
	{
		private static Type[] _defaultPrimitiveTypes = new[]
		{
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(Single),
			typeof(Double),
			typeof(Decimal),
			typeof(string),
			typeof(Guid),
			typeof(char),
			typeof(bool),
			typeof(DateTime),
			typeof(TimeSpan)
		};
		private static EnumerableConversionsConvention _bindEnumerableConversions
			= new EnumerableConversionsConvention();

		public DomainDefinition DomainDefinition { get; }
		public Type EntityType { get; }
		public Type ProjectionType { get; }
		public bool IsFirstPass { get; set; } = true;
		private readonly Stack<string> _prefixStack = new Stack<string>();
		private readonly Stack<Model> _modelStack = new Stack<Model>();

		public DataViewBuilder(Model sourceModel, Model targetModel, ViewConvention[] viewConventions,
			DomainDefinition domainDefinition, Type entityType, Type projectionType = null)
			: base(sourceModel, targetModel, viewConventions)
		{
			DomainDefinition = domainDefinition;
			EntityType = entityType;
			ProjectionType = projectionType;
		}

		public void PushModel(string fieldName, Model model)
		{
			_prefixStack.Push(fieldName);
			_modelStack.Push(model);
		}

		public void PopModel()
		{
			_prefixStack.Pop();
			_modelStack.Pop();
		}

		public override void DefineField(string viewFieldName, ModelBinding binding, Type fieldDataType, params object[] metadata)
		{
			if (_prefixStack.Count > 0)
			{
				viewFieldName = $"{string.Join("", _prefixStack)}{viewFieldName}";
				binding.ModelFieldPath = _prefixStack.Concat(binding.ModelFieldPath).ToArray();
			}

			base.DefineField(viewFieldName, binding, fieldDataType, metadata);

			if (!DomainDefinition.IsReadOnly)
			{
				var fieldDefinition = ViewDefinition.FieldDefinitions
					.First(q => q.Name == viewFieldName);

				var schemaDefinition = GetSchemaDefinition();
				var entityTable = schemaDefinition.GetEntityTableDefinition(true);
				entityTable.Fields.Add(fieldDefinition);
			}
		}

		public override bool IsFieldDefined(string viewFieldName)
		{
			if (_prefixStack.Count > 0)
			{
				viewFieldName = $"{string.Join("", _prefixStack)}{viewFieldName}";
			}

			return base.IsFieldDefined(viewFieldName);
		}

		public override FieldInfo FindSourceField(ModelField modelField, string name, bool caseSenitive = true, Type dataType = null)
		{
			if (_modelStack.Count > 0)
				return base.FindSourceField(_modelStack.Peek(), modelField, name, caseSenitive, dataType);
			return base.FindSourceField(modelField, name, caseSenitive, dataType);
		}

		public override FieldInfo FindSourceField(ModelField modelField, string[] path, bool caseSenitive = true, Type dataType = null)
		{
			if (_modelStack.Count > 0)
				return base.FindSourceField(_modelStack.Peek(), modelField, path, caseSenitive, dataType);
			return base.FindSourceField(modelField, path, caseSenitive, dataType);
		}

		public void ProcessModel(Model model)
		{
			foreach (var field in model.Fields)
			{
				foreach (var viewConvention in ViewDefinition.ViewConventions
					.OfType<ViewConvention<ViewBuilder>>())
				{
					if (!IsFirstPass && !viewConvention.PerformMultiplePasses)
						continue;
					if (!viewConvention.SupportedViewTypes.HasFlag(Mode))
						continue;
					if (viewConvention.SkipIfFieldDefined &&
						IsFieldDefined(field.Name))
						continue;
					viewConvention.MakeModelField(this, field);
				}
				foreach (var viewConvention in ViewDefinition.ViewConventions
					.OfType<ViewConvention<DataViewBuilder>>())
				{
					if (!IsFirstPass && !viewConvention.PerformMultiplePasses)
						continue;
					if (!viewConvention.SupportedViewTypes.HasFlag(Mode))
						continue;
					if (viewConvention.SkipIfFieldDefined &&
						IsFieldDefined(field.Name))
						continue;
					viewConvention.MakeModelField(this, field);
				}
			}

			_bindEnumerableConversions.FinalizeModel(this);

			foreach (var viewConvention in ViewDefinition.ViewConventions
					.OfType<ViewConvention<ViewBuilder>>())
			{
				if (!IsFirstPass && !viewConvention.PerformMultiplePasses)
					continue;
				viewConvention.FinalizeModel(this);
			}
			foreach (var viewConvention in ViewDefinition.ViewConventions
					.OfType<ViewConvention<DataViewBuilder>>())
			{
				if (!IsFirstPass && !viewConvention.PerformMultiplePasses)
					continue;
				viewConvention.FinalizeModel(this);
			}
		}

		public bool IsPrimitiveType(Type type)
		{
			return _defaultPrimitiveTypes.Contains(type);
		}

		private SchemaDefinition GetSchemaDefinition()
		{
			var schemaDefinition = GetSchemaDefinitionFor(EntityType);
			if (schemaDefinition == null)
			{
				schemaDefinition = new SchemaDefinition(
					ViewDefinition, EntityType, ProjectionType
					);

				DomainDefinition.SchemaDefinitions.Add(schemaDefinition);
			}
			return schemaDefinition;
		}

		private SchemaDefinition GetSchemaDefinitionFor(Type entityType)
		{
			return DomainDefinition
				.SchemaDefinitions.FirstOrDefault(q => q.EntityType == entityType);
		}
	}
}
