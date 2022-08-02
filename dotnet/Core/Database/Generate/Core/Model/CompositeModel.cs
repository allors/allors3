namespace Allors.Meta.Generation.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Meta;

    public abstract class CompositeModel : ObjectTypeModel
    {
        protected CompositeModel(MetaModel metaModel) : base(metaModel)
        {
        }

        protected abstract IComposite Composite { get; }

        // IComposite
        public IEnumerable<string> WorkspaceNames => this.Composite.WorkspaceNames;

        public IEnumerable<InterfaceModel> Supertypes => this.Composite.Supertypes.Select(this.MetaModel.Map);

        public IEnumerable<CompositeModel> Subtypes => this.Composite.Subtypes.Select(this.MetaModel.Map);

        public IEnumerable<ClassModel> Classes => this.Composite.Classes.Select(this.MetaModel.Map);

        public bool ExistExclusiveClass => this.Composite.ExistExclusiveClass;

        public IEnumerable<AssociationTypeModel> InheritedAssociationTypes => this.Composite.InheritedAssociationTypes.Select(this.MetaModel.Map);

        public IEnumerable<AssociationTypeModel> DatabaseAssociationTypes => this.Composite.DatabaseAssociationTypes.Select(this.MetaModel.Map);

        public IEnumerable<AssociationTypeModel> ExclusiveDatabaseAssociationTypes => this.Composite.ExclusiveDatabaseAssociationTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> InheritedRoleTypes => this.Composite.InheritedRoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> DatabaseRoleTypes => this.Composite.DatabaseRoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> ExclusiveDatabaseRoleTypes => this.Composite.ExclusiveDatabaseRoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<MethodTypeModel> MethodTypes => this.Composite.MethodTypes.Select(this.MetaModel.Map);

        public IEnumerable<MethodTypeModel> InheritedMethodTypes => this.Composite.InheritedMethodTypes.Select(this.MetaModel.Map);

        public IEnumerable<MethodTypeModel> ExclusiveMethodTypes => this.Composite.ExclusiveMethodTypes.Select(this.MetaModel.Map);

        public bool ExistDatabaseClass => this.Composite.ExistDatabaseClass;

        public IEnumerable<ClassModel> DatabaseClasses => this.Composite.DatabaseClasses.Select(this.MetaModel.Map);

        public bool ExistExclusiveDatabaseClass => this.Composite.ExistExclusiveDatabaseClass;

        public ClassModel ExclusiveDatabaseClass => this.MetaModel.Map(this.Composite.ExclusiveDatabaseClass);

        public bool IsRelationship => this.Composite.IsRelationship;

        // IComposite Extra
        public bool ExistDirectSupertypes => this.DirectSupertypes.Any();

        public bool ExistSupertypes => this.Supertypes.Any();

        public bool ExistAssociationTypes => this.AssociationTypes.Any();

        public bool ExistRoleTypes => this.AssociationTypes.Any();

        public bool ExistMethodTypes => this.MethodTypes.Any();

        public IEnumerable<InterfaceModel> DirectSupertypes => ((Composite)this.Composite).DirectSupertypes.Select(this.MetaModel.Map);

        public IEnumerable<CompositeModel> RelatedComposites =>
            this
                .Supertypes
                .Union(this.RoleTypes.Where(m => m.ObjectType.IsComposite).Select(v => v.ObjectType))
                .Union(this.AssociationTypes.Select(v => v.ObjectType)).Distinct()
                .Except(new[] { this })
                .Cast<CompositeModel>();

        public IEnumerable<AssociationTypeModel> AssociationTypes => ((Composite)this.Composite).AssociationTypes.Select(this.MetaModel.Map);

        public IEnumerable<AssociationTypeModel> ExclusiveAssociationTypes => ((Composite)this.Composite).ExclusiveAssociationTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> RoleTypes => ((Composite)this.Composite).RoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> ExclusiveRoleTypes => ((Composite)this.Composite).ExclusiveRoleTypes.Select(this.MetaModel.Map);

        public Origin AssignedOrigin => ((Composite)this.Composite).AssignedOrigin;

        public bool ExistClass => ((Composite)this.Composite).ExistClass;

        public IClassBase ExclusiveClass => ((Composite)this.Composite).ExclusiveClass;

        public IEnumerable<RoleTypeModel> UnitRoleTypes => this.RoleTypes.Where(roleType => roleType.ObjectType.IsUnit).ToArray();

        public IEnumerable<RoleTypeModel> UnitDatabaseRoleTypes => this.UnitRoleTypes.Where(v => v.Origin == Origin.Database).ToArray();

        public IEnumerable<RoleTypeModel> CompositeRoleTypes => this.RoleTypes.Where(roleType => roleType.ObjectType.IsComposite).ToArray();

        public IEnumerable<RoleTypeModel> CompositeDatabaseRoleTypes => this.CompositeRoleTypes.Where(v => v.Origin == Origin.Database).ToArray();

        public IEnumerable<RoleTypeModel> SortedExclusiveRoleTypes => this.ExclusiveRoleTypes.OrderBy(v => v.Name);

        public IEnumerable<RoleTypeModel> InheritedDatabaseRoleTypes => this.InheritedRoleTypes.Where(v => v.Origin == Origin.Database);

        public IEnumerable<AssociationTypeModel> InheritedDatabaseAssociationTypes => this.InheritedAssociationTypes.Where(v => v.Origin == Origin.Database);

        public IEnumerable<RoleTypeModel> ExclusiveCompositeRoleTypes => this.ExclusiveRoleTypes.Where(roleType => roleType.ObjectType.IsComposite);

        public IEnumerable<CompositeModel> DatabaseSubtypes => ((Composite)this.Composite).DatabaseSubtypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> ExclusiveRoleTypesWithDatabaseOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.Origin == Origin.Database);

        public IEnumerable<RoleTypeModel> ExclusiveRoleTypesWithSessionOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.Origin == Origin.Session);

        public IEnumerable<AssociationTypeModel> ExclusiveAssociationTypesWithDatabaseOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.Origin == Origin.Database);

        public IEnumerable<AssociationTypeModel> ExclusiveAssociationTypesWithSessionOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.Origin == Origin.Session);

        public IReadOnlyDictionary<string, IOrderedEnumerable<AssociationTypeModel>> WorkspaceAssociationTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.AssociationTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<AssociationTypeModel>> WorkspaceExclusiveAssociationTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.ExclusiveAssociationTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<AssociationTypeModel>> WorkspaceInheritedAssociationTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.InheritedAssociationTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceRoleTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.RoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceCompositeRoleTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.RoleTypes.Where(w => w.ObjectType.IsComposite && w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceInheritedRoleTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.InheritedRoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceExclusiveRoleTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.ExclusiveRoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceExclusiveRoleTypesWithDatabaseOriginByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.ExclusiveRoleTypes.Where(w => w.Origin == Origin.Database && w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceExclusiveRoleTypesWithWorkspaceOrSessionOriginByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.ExclusiveRoleTypes.Where(w => (w.Origin == Origin.Session) && w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceExclusiveCompositeRoleTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v,
                    v => this.ExclusiveRoleTypes.Where(w => w.ObjectType.IsComposite && w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<MethodTypeModel>> WorkspaceExclusiveMethodTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.ExclusiveMethodTypes.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<MethodTypeModel>> WorkspaceInheritedMethodTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.InheritedMethodTypes.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<MethodTypeModel>> WorkspaceMethodTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.MethodTypes.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<InterfaceModel>> WorkspaceDirectSupertypesByWorkspaceName => this.WorkspaceNames
                    .ToDictionary(v => v, v => this.DirectSupertypes.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<InterfaceModel>> WorkspaceSupertypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.Supertypes.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<CompositeModel>> WorkspaceSubtypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.Subtypes.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<CompositeModel>> WorkspaceRelatedCompositesByWorkspaceName => this.WorkspaceNames
                    .ToDictionary(v => v, v => this.RelatedComposites.Where(w => w.WorkspaceNames.Contains(v)).OrderBy(w => w.Tag));
    }
}
