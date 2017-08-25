namespace Niteco.Search.Fields
{
    public class CustomField
    {
        public virtual string Name { get; protected set; }

        public virtual float? Boost { get; protected set; }
    }
}
