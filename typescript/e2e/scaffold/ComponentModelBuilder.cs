namespace Scaffold
{
    using AngleSharp.Dom;

    public abstract class ComponentModelBuilder
    {
        public ComponentModelBuilder? Next { get; }

        protected ComponentModelBuilder(ComponentModelBuilder? next) => this.Next = next;

        public virtual ComponentModel? Build(IElement element) => this.Next?.Build(element);
    }
}
