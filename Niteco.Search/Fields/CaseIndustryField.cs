namespace Niteco.Search.Fields
{
    public class CaseIndustryField : CustomField
    {
        public const string FieldName = "NITECO_CASE_INDUSTRY";

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