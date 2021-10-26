namespace Allors.Meta.Generation.Model
{
    using System;
    using Database.Meta;

    public class DomainModel : MetaObjectModel, IMetaIdentifiableObjectModel
    {
        public DomainModel(MetaModel metaModel, IDomain domain) : base(metaModel) => this.Domain = domain;

        public IDomain Domain { get; }
        protected override IMetaObject MetaObject => this.Domain;

        // IMetaIdentifiableObject
        public Guid Id => this.Domain.Id;

        public string Tag => this.Domain.Tag;

        // IDomain
        public string Name => this.Domain.Name;
    }
}
