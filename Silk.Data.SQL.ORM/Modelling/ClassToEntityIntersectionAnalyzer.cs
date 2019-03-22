using Silk.Data.Modelling;
using Silk.Data.Modelling.Analysis;
using Silk.Data.Modelling.Analysis.CandidateSources;
using Silk.Data.Modelling.Analysis.Rules;
using Silk.Data.Modelling.GenericDispatch;
using Silk.Data.SQL.ORM.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Silk.Data.SQL.ORM.Modelling
{
	public class ClassToEntityIntersectionAnalyzer
	{
		private readonly ICollection<IIntersectCandidateSource<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> _candidateSources
			= new List<IIntersectCandidateSource<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>();

		private readonly ICollection<IIntersectionRule<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> _intersectionRules
			= new List<IIntersectionRule<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>();

		public ClassToEntityIntersectionAnalyzer(
			IEnumerable<IIntersectCandidateSource<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> intersectCandidateSources,
			IEnumerable<IIntersectionRule<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> intersectionRules
			)
		{
			foreach (var source in intersectCandidateSources)
				_candidateSources.Add(source);
			foreach (var rule in intersectionRules)
				_intersectionRules.Add(rule);
		}

		protected virtual IntersectCandidate<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>[] GetIntersectCandidates(
			ViewIntersectionModel leftModel, EntityModel rightModel)
			=> _candidateSources.SelectMany(source => source.GetIntersectCandidates(leftModel, rightModel)).ToArray();

		/// <summary>
		/// Populate the IntersectAnalysis with IntersectFields from valid rules matches from the provided candidates.
		/// </summary>
		/// <param name="analysis"></param>
		/// <param name="intersectCandidates"></param>
		protected virtual void ApplyValidIntersectionCandidates(IntersectAnalysis analysis, IntersectCandidate<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>[] intersectCandidates)
		{
			foreach (var candidate in intersectCandidates)
			{
				if (analysis.IsLeftFieldAlreadyPaired(candidate))
					continue;

				foreach (var rule in _intersectionRules)
				{
					if (rule.IsValidIntersection(candidate, out var intersectedFields))
					{
						analysis.AddIntersectedFields(intersectedFields);
						break;
					}
				}
			}
		}

		protected virtual IEnumerable<IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> GetInvalidFields(
			IntersectAnalysis intersectAnalysis
			)
		{
			foreach (var intersectedFields in intersectAnalysis.IntersectedFields)
			{
				if (intersectedFields.LeftField.FieldDataType != intersectedFields.RightField.FieldDataType &&
					SqlTypeHelper.GetDataType(intersectedFields.RightField.FieldDataType) == null)
				{
					yield return intersectedFields;
				}
			}
		}

		private ViewIntersectionField BuildReplacement(
			IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField> intersectedFields,
			FieldBuilder fieldBuilder
			)
		{
			intersectedFields.Dispatch(fieldBuilder);
			return fieldBuilder.Field;
		}

		public IIntersection<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField> CreateIntersection(
			TypeModel leftModel, EntityModel rightModel
			)
		{
			var fieldBuilder = new FieldBuilder();
			var viewIntersectionModel = ViewIntersectionModel.FromTypeModel(leftModel);
			while (true)
			{
				var intersectCandidates = GetIntersectCandidates(viewIntersectionModel, rightModel);
				var analysisCtx = new IntersectAnalysis(viewIntersectionModel, rightModel);
				ApplyValidIntersectionCandidates(analysisCtx, intersectCandidates);

				var invalidFields = GetInvalidFields(analysisCtx).ToArray();
				if (invalidFields.Length == 0)
				{
					//  remove any unbound fields
					return new Intersection(viewIntersectionModel, rightModel, analysisCtx.IntersectedFields.ToArray());
				}

				foreach (var invalidField in invalidFields)
				{
					invalidField.LeftField.Replace(BuildReplacement(invalidField, fieldBuilder));
				}
			}
		}

		protected class FieldBuilder : IIntersectedFieldsGenericExecutor
		{
			public ViewIntersectionField Field { get; private set; }

			void IIntersectedFieldsGenericExecutor.Execute<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData>(
				IntersectedFields<TLeftModel, TLeftField, TRightModel, TRightField, TLeftData, TRightData> intersectedFields
				)
			{
				var intersect = intersectedFields as IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField, TLeftData, TRightData>;
				Field = new ConvertedViewIntersectionField<TLeftData, TRightData>(
					intersect.LeftField.FieldName, intersect.LeftField.CanRead, intersect.LeftField.CanWrite,
					intersect.LeftField.DeclaringTypeModel, intersect.LeftField.OriginPropertyField,
					intersect.LeftPath, intersect.GetConvertDelegate()
					);
			}
		}

		/// <summary>
		/// Analysis state object.
		/// </summary>
		protected class IntersectAnalysis
		{
			private List<IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> _intersectedFields
				= new List<IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>>();

			public ViewIntersectionModel LeftModel { get; }
			public EntityModel RightModel { get; }
			public IReadOnlyList<IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>> IntersectedFields => _intersectedFields;

			public IntersectAnalysis(ViewIntersectionModel leftModel, EntityModel rightModel)
			{
				LeftModel = leftModel;
				RightModel = rightModel;
			}

			public virtual bool IsLeftFieldAlreadyPaired(IntersectCandidate<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField> intersectCandidate)
				=> _intersectedFields.Any(intersectedFields => ReferenceEquals(intersectedFields.LeftField, intersectCandidate.LeftField));

			public virtual void AddIntersectedFields(IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField> intersectedFields)
				=> _intersectedFields.Add(intersectedFields);
		}

		protected class Intersection :
			IntersectionBase<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>
		{
			public Intersection(ViewIntersectionModel leftModel, EntityModel rightModel,
				IntersectedFields<ViewIntersectionModel, ViewIntersectionField, EntityModel, EntityField>[] intersectedFields) :
				base(leftModel, rightModel, intersectedFields)
			{
			}
		}
	}
}
