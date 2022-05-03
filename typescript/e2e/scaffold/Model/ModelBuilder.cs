namespace Scaffold
{
    public abstract class ModelBuilder
    {
        public ModelBuilder? Next { get; }

        public abstract Template Template { get; }

        public abstract string Namespace { get; }

        public abstract ComponentModelBuilder ComponentModelBuilder { get; }

        protected ModelBuilder(ModelBuilder? next) => this.Next = next;

        public virtual Model? Build(FileInfo fileInfo) => this.Next?.Build(fileInfo);
    }
}
