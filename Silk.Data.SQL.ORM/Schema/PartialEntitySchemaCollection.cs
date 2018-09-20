﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silk.Data.SQL.ORM.Schema
{
	/// <summary>
	/// Collection of defined entity types and their fields that are primitive SQL types.
	/// </summary>
	public class PartialEntitySchemaCollection : Dictionary<Type, PartialEntitySchema>
	{
		public PartialEntitySchemaCollection(IEnumerable<Type> definedEntityTypes)
		{
			foreach (var type in definedEntityTypes)
				Add(type, null);
		}

		/// <summary>
		/// Determines if an entity type has been defined in the schema builder.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool IsEntityTypeDefined<T>() => IsEntityTypeDefined(typeof(T));

		/// <summary>
		/// Determines if an entity type has been defined in the schema builder.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool IsEntityTypeDefined(Type type)
		{
			return ContainsKey(type);
		}

		public IEnumerable<IEntityField> GetEntityPrimaryKeys<T>() => GetEntityPrimaryKeys(typeof(T));

		public IEnumerable<IEntityField> GetEntityPrimaryKeys(Type type)
		{
			if (TryGetValue(type, out var partialEntitySchema))
			{
				return partialEntitySchema.EntityFields.Where(q => q.IsPrimaryKey);
			}
			return null;
		}
	}
}