namespace Allors.Meta.Generation.Model
{
    using Database.Meta;

    public abstract class MetaObjectModel
    {
        public const Origin DefaultOrigin = Origin.Database;

        protected MetaObjectModel(MetaModel metaModel) => this.MetaModel = metaModel;

        public MetaModel MetaModel { get; }

        public Origin Origin => this.MetaObject.Origin;

        protected abstract IMetaObject MetaObject { get; }

        public int OriginAsInt => (int)this.Origin;

        public bool HasDatabaseOrigin => this.Origin == Origin.Database;

        public bool HasSessionOrigin => this.Origin == Origin.Session;

        public bool IsDefaultOrigin => this.Origin == DefaultOrigin;

        public override string ToString() => this.MetaObject.ToString();
    }
}
