namespace Allors.E2E.Angular.Material.Person
{
    using E2E;
    using Table;

    public class PersonListComponent : ContainerComponent
    {
        public PersonListComponent(IComponent container) : base(container) { }

        public AllorsMaterialTableComponent Table { get; }
    }
}
