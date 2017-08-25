using Lucene.Net.Documents;

namespace Niteco.Search.IndexingService
{
	public class ScoreDocument
	{
		internal Document Document
		{
			get;
			set;
		}
		internal float Score
		{
			get;
			set;
		}
		internal ScoreDocument(Document document, float score)
		{
			this.Document = document;
			this.Score = score;
		}
	}
}
