namespace Allors.Meta.Generation.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;

    public sealed class ClassModel : CompositeModel
    {
        public ClassModel(MetaModel metaModel, IClass @class) : base(metaModel) => this.Class = @class;

        public IClass Class { get; }
        protected override IMetaObject MetaObject => this.Class;
        protected override IObjectType ObjectType => this.Class;
        protected override IComposite Composite => this.Class;

        // IClass
        public IEnumerable<RoleTypeModel> OverriddenRequiredRoleTypes => this.Class.OverriddenRequiredRoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> OverriddenUniqueRoleTypes => this.Class.OverriddenUniqueRoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> RequiredRoleTypes => this.Class.RequiredRoleTypes.Select(this.MetaModel.Map);

        public IEnumerable<RoleTypeModel> UniqueRoleTypes => this.Class.UniqueRoleTypes.Select(this.MetaModel.Map);

        // IClass Extra
        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceOverriddenRequiredByWorkspaceName => this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.OverriddenRequiredRoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));

        public IReadOnlyDictionary<string, IOrderedEnumerable<RoleTypeModel>> WorkspaceOverriddenUniqueByWorkspaceName => this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.OverriddenUniqueRoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).OrderBy(w => w.RelationType.Tag));
    }
}
