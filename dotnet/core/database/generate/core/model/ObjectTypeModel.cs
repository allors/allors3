namespace Allors.Meta.Generation.Model
{
    using System;
    using Database.Meta;

    public abstract class ObjectTypeModel : MetaObjectModel, IMetaIdentifiableObjectModel
    {
        protected ObjectTypeModel(MetaModel metaModel) : base(metaModel)
        {
        }

        protected abstract IObjectType ObjectType { get; }

        // IMetaIdentifiableObject
        public Guid Id => this.ObjectType.Id;

        public string Tag => this.ObjectType.Tag;

        // IObjectType
        public bool IsUnit => this.ObjectType.IsUnit;

        public bool IsComposite => this.ObjectType.IsComposite;

        public bool IsInterface => this.ObjectType.IsInterface;

        public bool IsClass => this.ObjectType.IsClass;

        public string SingularName => this.ObjectType.SingularName;

        public string PluralName => this.ObjectType.PluralName;

        public string Name => this.ObjectType.Name;

        public Type ClrType => this.ObjectType.ClrType;

        public bool ExistAssignedPluralName => ((ObjectType)this.ObjectType).ExistAssignedPluralName;
    }
}
