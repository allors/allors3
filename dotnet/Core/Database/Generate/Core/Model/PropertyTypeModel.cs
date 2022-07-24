namespace Allors.Meta.Generation.Model
{
    using Database.Meta;

    public abstract class PropertyTypeModel : OperandTypeModel
    {
        protected PropertyTypeModel(MetaModel metaModel) : base(metaModel)
        {
        }

        protected abstract IPropertyType PropertyType { get; }

        // IPropertyType
        public ObjectTypeModel ObjectType => this.MetaModel.Map(this.PropertyType.ObjectType);

        public string Name => this.PropertyType.Name;

        public string SingularName => this.PropertyType.SingularName;

        public string SingularFullName => this.PropertyType.SingularFullName;

        public string PluralName => this.PropertyType.PluralName;

        public string PluralFullName => this.PropertyType.PluralFullName;

        public bool IsOne => this.PropertyType.IsOne;

        public bool IsMany => this.PropertyType.IsMany;
    }
}
