namespace Niteco.Search.IndexingService
{
	public class InternalServerErrorEventArgs : System.EventArgs
	{
		public string ErrorMessage
		{
			get;
			set;
		}
		public InternalServerErrorEventArgs(string errorMessage)
		{
			this.ErrorMessage = errorMessage;
		}
	}
}
