using Silk.Data.Modelling;
using System;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Helper class for visiting fields declared on entity types.
	/// </summary>
	internal static class EntityTypeVisitor
	{
		public delegate void VisitNode(IPropertyField entityPropertyField, Span<string> path);
		public delegate bool ContinueIntoNode(IPropertyField entityPropertyField, Span<string> path);

		public static void Visit<T>(TypeModel entityTypeModel, EntitySchemaDefinition<T> entitySchemaDefinition, VisitNode visitNodeCallback,
			ContinueIntoNode continueIntoNodeCallback = null)
			where T : class
		{
			var pathArray = new string[255];
			DoVisit(entityTypeModel, 0);

			void DoVisit(TypeModel currentTypeModel, int pathLength)
			{
				CheckPathArrayBounds(pathLength);
				var path = new Span<string>(pathArray, 0, pathLength);
				foreach (var entityPropertyField in currentTypeModel.Fields)
				{
					if (entityPropertyField.IsEnumerable || !entitySchemaDefinition.IsModelled(entityPropertyField))
						continue;

					pathArray[pathLength] = entityPropertyField.FieldName;
					var pathWithFieldName = new Span<string>(pathArray, 0, pathLength + 1);

					if (continueIntoNodeCallback != null)
					{
						if (!continueIntoNodeCallback(entityPropertyField, pathWithFieldName))
							continue;
					}

					visitNodeCallback(entityPropertyField, pathWithFieldName);

					if (!SqlTypeHelper.IsSqlPrimitiveType(entityPropertyField.FieldType))
					{
						var propertyTypeModel = TypeModel.GetModelOf(entityPropertyField.FieldType);
						DoVisit(propertyTypeModel, pathLength + 1);
					}
				}
			}

			void CheckPathArrayBounds(int length)
			{
				if (pathArray.Length == length)
				{
					var newArray = new string[pathArray.Length + 255];
					Array.Copy(pathArray, newArray, pathArray.Length);
					pathArray = newArray;
				}
			}
		}
	}
}
