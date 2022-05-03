namespace Scaffold
{
    public abstract class ComponentModel
    {
        public abstract string Property { get; protected set; }

        public abstract string Type { get; }

        public abstract string Init { get; }

        public abstract void ElevatePropertyName(ISet<string> properties);
    }
}
