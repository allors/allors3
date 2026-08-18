namespace Allors.Workspace.Blazor.Validation
{
    using System;
    using System.Linq;
    using Meta;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;

    public abstract class CustomValidator : ComponentBase
    {
        [CascadingParameter]
        public EditContext EditContext { get; set; }

        [CascadingParameter]
        public Fields Fields { get; set; }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            if (this.EditContext == null)
            {
                throw new InvalidOperationException($"{nameof(DefaultValidator)} requires cascading parameters {nameof(this.EditContext)} and {nameof(this.Fields)}.");
            }

            var messages = new ValidationMessageStore(this.EditContext);

            // Perform object-level validation on request
            this.EditContext.OnValidationRequested += (sender, eventArgs) =>
            {
                messages.Clear();
                foreach (var field in this.Fields.Items)
                {
                    this.Validate(field, messages);
                }

                this.EditContext.NotifyValidationStateChanged();
            };

            // Perform per-field validation on each field edit
            this.EditContext.OnFieldChanged += (sender, eventArgs) =>
            {
                foreach (var field in this.Fields.Items.Where(v => v == eventArgs.FieldIdentifier.Model))
                {
                    this.Validate(field, messages);
                }

                this.EditContext.NotifyValidationStateChanged();
            };
        }

        // ExistRole covers unit, one and many roles; GetRole returns a lazy projection for a many
        // role, so it can not be inspected as a collection.
        protected void AssertExists(IField field, ValidationMessageStore messages, IPropertyType propertyType)
        {
            if (field.PropertyType == propertyType && field.PropertyType is IRoleType roleType)
            {
                if (!field.Object.Strategy.ExistRole(roleType))
                {
                    this.AddShouldExistMessage(field, messages);
                }
            }
        }

        protected void AssertNotExists(IField field, ValidationMessageStore messages, IPropertyType propertyType)
        {
            if (field.PropertyType == propertyType && field.PropertyType is IRoleType roleType)
            {
                if (field.Object.Strategy.ExistRole(roleType))
                {
                    this.AddShouldNotExistMessage(field, messages);
                }
            }
        }

        protected virtual void AddShouldExistMessage(IField field, ValidationMessageStore messages) => messages.Add(field.FieldIdentifier, $"{field.PropertyType.Name} is required");

        protected virtual void AddShouldNotExistMessage(IField field, ValidationMessageStore messages) => messages.Add(field.FieldIdentifier, $"{field.PropertyType.Name} is not allowed");

        protected abstract void Validate(IField field, ValidationMessageStore messages);
    }
}
