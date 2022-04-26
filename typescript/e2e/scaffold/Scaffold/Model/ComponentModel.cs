namespace Scaffold
{
    using AngleSharp.Dom;

    public class ComponentModel
    {
        public string Name { get; }

        public ComponentModel(IElement element)
        {
            this.Name = element.TagName;
        }
    }
}
