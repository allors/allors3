namespace Tests
{
    using Angular.Components;

    public class PeopleComponent : ContainerComponent
    {
        public PeopleComponent(IComponent container) : base(container) { }

        public AllorsMaterialAutocompleteComponent AutocompleteDerivedFilter => new AllorsMaterialAutocompleteComponent(this, this.M.Data.AutocompleteDerivedFilter);
        public object Table { get; }
    }
}
