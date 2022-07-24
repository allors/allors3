namespace Allors.Meta.Generation.Model
{
    using Allors.Database.Meta;

    public class InterfaceModel : CompositeModel
    {
        public InterfaceModel(MetaModel metaModel, IInterface @interface) : base(metaModel) => this.Interface = @interface;

        public IInterface Interface { get; }
        protected override IMetaObject MetaObject => this.Interface;
        protected override IObjectType ObjectType => this.Interface;
        protected override IComposite Composite => this.Interface;
    }
}
