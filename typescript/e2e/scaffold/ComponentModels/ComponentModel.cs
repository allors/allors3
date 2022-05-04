namespace Scaffold
{
    public abstract class ComponentModel
    {
        public abstract string Property { get; protected set; }

        public abstract string Type { get; }

        public abstract string Init { get; protected set; }

        public abstract void Elevate(ISet<string> properties);
    }
}
