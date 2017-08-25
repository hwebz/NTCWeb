namespace Niteco.Search.Fields
{
    public class StartPublishedField : CustomField
    {
        public const string FieldName = "NITECO_START_PUBLISHED";

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
