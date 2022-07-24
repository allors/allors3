namespace Allors.Meta.Generation.Model
{
    using System.Collections.Generic;
    using Database.Meta;

    public abstract class OperandTypeModel : MetaObjectModel
    {
        protected OperandTypeModel(MetaModel metaModel) : base(metaModel) { }

        protected abstract IOperandType OperandType { get; }

        public IEnumerable<string> WorkspaceNames => this.OperandType.WorkspaceNames;

        public IEnumerable<string> AssignedWorkspaceNames => this.OperandType.AssignedWorkspaceNames;
    }
}
