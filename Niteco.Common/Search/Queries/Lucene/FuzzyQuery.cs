using System;
using System.Globalization;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class FuzzyQuery : FieldQuery
	{
		public float SimilarityFactor
		{
			get;
			set;
		}
		public FuzzyQuery(string word, Field field, float similarityFactor) : base(word, field)
		{
			this.ValidateSimilarityFactor(similarityFactor, "similarityFactor");
			this.SimilarityFactor = similarityFactor;
		}
		public FuzzyQuery(string word, float similarityFactor) : base(word, Field.Default)
		{
			this.ValidateSimilarityFactor(similarityFactor, "similarityFactor");
			this.SimilarityFactor = similarityFactor;
		}
		public override string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.GetFieldNameForField(base.Field));
			stringBuilder.Append(":(");
			stringBuilder.Append(LuceneHelpers.Escape(base.Expression));
			stringBuilder.Append("~");
			stringBuilder.Append(this.SimilarityFactor.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", "."));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
		private void ValidateSimilarityFactor(float similarityFactor, string argumentName)
		{
			if (similarityFactor < 0f || similarityFactor > 1f)
			{
				throw new System.ArgumentException("The similarity factor must be between 0 and 1", argumentName);
			}
		}
	}
}
