namespace Niteco.Search.Fields
{
    public class EventTagField : CustomField
    {
        public const string FieldName = "NITECO_EVENT_TAG";

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