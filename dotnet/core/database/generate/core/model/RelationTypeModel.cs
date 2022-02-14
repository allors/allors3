namespace Allors.Meta.Generation.Model
{
    using System;
    using System.Collections.Generic;
    using Allors.Database.Meta;

    public class RelationTypeModel : MetaObjectModel, IMetaIdentifiableObjectModel
    {
        public RelationTypeModel(MetaModel metaModel, IRelationType relationType) : base(metaModel) => this.RelationType = relationType;

        public IRelationType RelationType { get; }
        protected override IMetaObject MetaObject => this.RelationType;

        // IMetaIdentifiableObject
        public Guid Id => this.RelationType.Id;

        public string Tag => this.RelationType.Tag;

        // IRelationType
        public AssociationTypeModel AssociationType => this.MetaModel.Map(this.RelationType.AssociationType);

        public RoleTypeModel RoleType => this.MetaModel.Map(this.RelationType.RoleType);

        public Multiplicity Multiplicity => this.RelationType.Multiplicity;

        public bool IsOneToOne => this.RelationType.IsOneToOne;

        public bool IsOneToMany => this.RelationType.IsOneToMany;

        public bool IsManyToOne => this.RelationType.IsManyToOne;

        public bool IsManyToMany => this.RelationType.IsManyToMany;

        public bool ExistExclusiveDatabaseClasses => this.RelationType.ExistExclusiveDatabaseClasses;

        public bool IsIndexed => this.RelationType.IsIndexed;

        public bool IsDerived => this.RelationType.IsDerived;

        public IEnumerable<string> WorkspaceNames => this.RelationType.WorkspaceNames;

        public IEnumerable<string> AssignedWorkspaceNames => this.RelationType.AssignedWorkspaceNames;

        public string Name => ((RelationType)this.RelationType).Name;
    }
}
