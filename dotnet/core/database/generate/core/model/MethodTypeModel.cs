namespace Allors.Meta.Generation.Model
{
    using System;
    using Allors.Database.Meta;

    public class MethodTypeModel : OperandTypeModel, IMetaIdentifiableObjectModel
    {
        public MethodTypeModel(MetaModel metaModel, IMethodType methodType) : base(metaModel) => this.MethodType = methodType;

        public IMethodType MethodType { get; }
        protected override IMetaObject MetaObject => this.MethodType;
        protected override IOperandType OperandType => this.MethodType;

        // IMetaIdentifiableObject
        public Guid Id => this.MethodType.Id;

        public string Tag => this.MethodType.Tag;

        // IMethodType
        public CompositeModel ObjectType => this.MetaModel.Map(this.MethodType.ObjectType);

        public string Name => this.MethodType.Name;

        public string FullName => this.MethodType.FullName;
    }
}
