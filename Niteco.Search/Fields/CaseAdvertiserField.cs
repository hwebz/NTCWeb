namespace Niteco.Search.Fields
{
    public class CaseAdvertiserField : CustomField
    {
        public const string FieldName = "NITECO_CASE_ADVERTISER";

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