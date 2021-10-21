namespace Allors.Meta.Generation.Model
{
    using Allors.Database.Meta;

    public class RoleTypeModel : PropertyTypeModel
    {
        public RoleTypeModel(MetaModel metaModel, IRoleType roleType) : base(metaModel) => this.RoleType = roleType;

        public IRoleType RoleType { get; }
        protected override IMetaObject MetaObject => this.RoleType;
        protected override IOperandType OperandType => this.RoleType;
        protected override IPropertyType PropertyType => this.RoleType;

        // IRoleType
        public AssociationTypeModel AssociationType => this.MetaModel.Map(this.RoleType.AssociationType);

        public RelationTypeModel RelationType => this.MetaModel.Map(this.RoleType.RelationType);

        public string FullName => this.RoleType.FullName;

        public bool ExistAssignedSingularName => ((RoleType)this.RoleType).ExistAssignedSingularName;

        public bool ExistAssignedPluralName => ((RoleType)this.RoleType).ExistAssignedPluralName;

        public int? Size => this.RoleType.Size;

        public int? Precision => this.RoleType.Precision;

        public int? Scale => this.RoleType.Scale;

        public bool IsRequired => this.RoleType.IsRequired;

        public bool IsUnique => this.RoleType.IsUnique;

        public string MediaType => this.RoleType.MediaType;
    }
}
