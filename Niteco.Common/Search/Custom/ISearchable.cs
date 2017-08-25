namespace Niteco.Common.Search.Custom
{
    public interface ISearchable
    {
        bool AllowIndexChildren { get; }

        bool IsSearchable { get; }
    }
}
