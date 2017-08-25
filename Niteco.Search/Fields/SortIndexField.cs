namespace Niteco.Search.Fields
{
    public class SortIndexField : CustomField
    {
        public const string FieldName = "NITECO_SORT_INDEX";

        public override string Name
        {
            get
            {
                return FieldName;
            }
        }

        public override float? Boost
        {
            get
            {
                return 1.0f;
            }
        }
    }
}
