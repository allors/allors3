// <copyright file="CustomValidatorTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Blazor.Bootstrap.Tests
{
    using System;
    using System.Linq;
    using Allors.Workspace.Blazor.Validation;
    using Allors.Workspace.Meta;
    using Microsoft.AspNetCore.Components.Forms;
    using Xunit;
    using Organisation = Allors.Workspace.Domain.Organisation;
    using Person = Allors.Workspace.Domain.Person;

    /// <summary>
    /// Covers the <see cref="CustomValidator"/> assertion helpers, in particular for many valued
    /// roles: Strategy.GetRole hands back a lazy projection, never an ICollection.
    /// </summary>
    public class CustomValidatorTests : IClassFixture<WorkspaceFixture>
    {
        private readonly WorkspaceFixture fixture;

        public CustomValidatorTests(WorkspaceFixture fixture) => this.fixture = fixture;

        [Fact]
        public void AssertExistsReportsAnEmptyManyRole()
        {
            var person = this.NewPerson();

            Assert.NotEmpty(AssertExists(person, this.fixture.M.Person.CycleMany));
        }

        [Fact]
        public void AssertExistsAcceptsAFilledManyRole()
        {
            var person = this.NewPerson(out var organisation);
            person.AddCycleMany(organisation);

            Assert.Empty(AssertExists(person, this.fixture.M.Person.CycleMany));
        }

        [Fact]
        public void AssertNotExistsReportsAFilledManyRole()
        {
            var person = this.NewPerson(out var organisation);
            person.AddCycleMany(organisation);

            Assert.NotEmpty(AssertNotExists(person, this.fixture.M.Person.CycleMany));
        }

        [Fact]
        public void AssertNotExistsAcceptsAnEmptyManyRole()
        {
            var person = this.NewPerson();

            Assert.Empty(AssertNotExists(person, this.fixture.M.Person.CycleMany));
        }

        [Fact]
        public void AssertExistsReportsAnEmptyOneRole()
        {
            var person = this.NewPerson();

            Assert.NotEmpty(AssertExists(person, this.fixture.M.Person.CycleOne));
        }

        [Fact]
        public void AssertExistsAcceptsAFilledOneRole()
        {
            var person = this.NewPerson(out var organisation);
            person.CycleOne = organisation;

            Assert.Empty(AssertExists(person, this.fixture.M.Person.CycleOne));
        }

        [Fact]
        public void AssertNotExistsReportsAFilledOneRole()
        {
            var person = this.NewPerson(out var organisation);
            person.CycleOne = organisation;

            Assert.NotEmpty(AssertNotExists(person, this.fixture.M.Person.CycleOne));
        }

        [Fact]
        public void AssertExistsReportsAnEmptyUnitRole()
        {
            var person = this.NewPerson();

            Assert.NotEmpty(AssertExists(person, this.fixture.M.Person.FirstName));
        }

        [Fact]
        public void AssertExistsAcceptsAFilledUnitRole()
        {
            var person = this.NewPerson();
            person.FirstName = "Jane";

            Assert.Empty(AssertExists(person, this.fixture.M.Person.FirstName));
        }

        [Fact]
        public void AssertExistsIgnoresAnAssociationType()
        {
            var person = this.NewPerson();

            Assert.Empty(AssertExists(person, this.fixture.M.Person.OrganisationsWhereCycleMany));
        }

        [Fact]
        public void AssertExistsIgnoresAnotherPropertyType()
        {
            var person = this.NewPerson();

            // The field is bound to CycleMany, but CycleOne is asserted, so the field is not this one.
            var field = new StubField(person, this.fixture.M.Person.CycleMany);

            Assert.Empty(Run(field, (validator, messages) =>
                validator.AssertExists(field, messages, this.fixture.M.Person.CycleOne)));
        }

        [Fact]
        public void AssertExistsMessageNamesTheRole()
        {
            var person = this.NewPerson();

            Assert.Equal(new[] { "CycleMany is required" }, AssertExists(person, this.fixture.M.Person.CycleMany));
        }

        [Fact]
        public void AssertNotExistsMessageNamesTheRole()
        {
            var person = this.NewPerson(out var organisation);
            person.AddCycleMany(organisation);

            Assert.Equal(new[] { "CycleMany is not allowed" }, AssertNotExists(person, this.fixture.M.Person.CycleMany));
        }

        /// <summary>
        /// BlazorStrap inputs take part in the EditContext themselves; during a validation request
        /// they notify a field change whose model is the input, not one of the tracked fields.
        /// </summary>
        [Fact]
        public void FieldChangedForAnUntrackedModelIsIgnored()
        {
            var person = this.NewPerson();
            var field = new StubField(person, this.fixture.M.Person.CycleMany);
            var editContext = new EditContext(person);
            Subscribe(editContext, field);

            var exception = Record.Exception(() =>
                editContext.NotifyFieldChanged(new FieldIdentifier(new UntrackedInput(), "_value")));

            Assert.Null(exception);
            Assert.Empty(editContext.GetValidationMessages());
        }

        [Fact]
        public void FieldChangedForATrackedFieldValidatesIt()
        {
            var person = this.NewPerson();
            var field = new StubField(person, this.fixture.M.Person.CycleMany);
            var editContext = new EditContext(person);
            Subscribe(editContext, field);

            editContext.NotifyFieldChanged(field.FieldIdentifier);

            Assert.Equal(new[] { "CycleMany is required" }, editContext.GetValidationMessages(field.FieldIdentifier).ToArray());
        }

        private Person NewPerson() => this.fixture.Workspace.CreateSession().Create<Person>();

        /// <summary>
        /// The organisation shares the person's session; roles can only be set between strategies
        /// of the same session.
        /// </summary>
        private Person NewPerson(out Organisation organisation)
        {
            var session = this.fixture.Workspace.CreateSession();
            organisation = session.Create<Organisation>();
            return session.Create<Person>();
        }

        private static string[] AssertExists(IObject @object, IPropertyType propertyType)
        {
            var field = new StubField(@object, propertyType);
            return Run(field, (validator, messages) => validator.AssertExists(field, messages, propertyType));
        }

        private static string[] AssertNotExists(IObject @object, IPropertyType propertyType)
        {
            var field = new StubField(@object, propertyType);
            return Run(field, (validator, messages) => validator.AssertNotExists(field, messages, propertyType));
        }

        private static string[] Run(StubField field, Action<TestValidator, ValidationMessageStore> assert)
        {
            var editContext = new EditContext(field.Object);
            var messages = new ValidationMessageStore(editContext);

            assert(new TestValidator(), messages);

            return editContext.GetValidationMessages(field.FieldIdentifier).ToArray();
        }

        /// <summary>
        /// Hooks a validator onto the EditContext the way a rendered one does; CustomValidator
        /// subscribes to the context in OnInitialized.
        /// </summary>
        private static void Subscribe(EditContext editContext, params IField[] fields)
        {
            var validationFields = new Fields();
            foreach (var field in fields)
            {
                validationFields.Add(field);
            }

            new RequiredValidator { EditContext = editContext, Fields = validationFields }.Initialize();
        }

        /// <summary>
        /// Exposes the protected assertion helpers; the validator itself is never rendered, the
        /// helpers do not depend on any component state.
        /// </summary>
        private sealed class TestValidator : CustomValidator
        {
            public new void AssertExists(IField field, ValidationMessageStore messages, IPropertyType propertyType) =>
                base.AssertExists(field, messages, propertyType);

            public new void AssertNotExists(IField field, ValidationMessageStore messages, IPropertyType propertyType) =>
                base.AssertNotExists(field, messages, propertyType);

            protected override void Validate(IField field, ValidationMessageStore messages)
            {
            }
        }

        /// <summary>
        /// Asserts each tracked field against its own property type, so validating a field with an
        /// empty role produces a message.
        /// </summary>
        private sealed class RequiredValidator : CustomValidator
        {
            public void Initialize() => this.OnInitialized();

            protected override void Validate(IField field, ValidationMessageStore messages) =>
                this.AssertExists(field, messages, field.PropertyType);
        }

        /// <summary>
        /// Stands in for a BlazorStrap input: it notifies the same EditContext, but it is not one
        /// of the tracked fields.
        /// </summary>
        private sealed class UntrackedInput
        {
        }

        private sealed class StubField : IField
        {
            public StubField(IObject @object, IPropertyType propertyType)
            {
                this.Object = @object;
                this.PropertyType = propertyType;
            }

            public IObject Object { get; }

            public IPropertyType PropertyType { get; }

            public object Model { get; set; }

            public FieldIdentifier FieldIdentifier => new FieldIdentifier(this, nameof(this.Model));

            public void Validate(ValidationMessageStore messages)
            {
            }
        }
    }
}
