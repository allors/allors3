// <copyright file="BootstrapCheckboxRoleTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Blazor.Bootstrap.Tests
{
    using Allors.Workspace.Blazor.Bootstrap.Forms.Roles;
    using Bunit;
    using Microsoft.AspNetCore.Components.Forms;
    using Xunit;
    using Person = Allors.Workspace.Domain.Person;

    public class BootstrapCheckboxRoleTests : BootstrapRoleTestContext, IClassFixture<WorkspaceFixture>
    {
        private readonly WorkspaceFixture fixture;

        public BootstrapCheckboxRoleTests(WorkspaceFixture fixture) => this.fixture = fixture;

        [Fact]
        public void RendersUncheckedWhenRoleIsFalse()
        {
            var person = this.NewPerson(out var roleType);
            person.IsStudent = false;

            var cut = this.Render(person, roleType);

            Assert.False(cut.Find("input").HasAttribute("checked"));
        }

        [Fact]
        public void RendersCheckedWhenRoleIsTrue()
        {
            var person = this.NewPerson(out var roleType);
            person.IsStudent = true;

            var cut = this.Render(person, roleType);

            Assert.True(cut.Find("input").HasAttribute("checked"));
        }

        [Fact]
        public void ClickTogglesTheRole()
        {
            var person = this.NewPerson(out var roleType);
            person.IsStudent = false;

            var cut = this.Render(person, roleType);

            cut.Find("input").Click();
            Assert.True(person.IsStudent);
            Assert.True(cut.Find("input").HasAttribute("checked"));

            cut.Find("input").Click();
            Assert.False(person.IsStudent ?? false);
            Assert.False(cut.Find("input").HasAttribute("checked"));
        }

        private Person NewPerson(out Allors.Workspace.Meta.IRoleType roleType)
        {
            var session = this.fixture.Workspace.CreateSession();
            var person = session.Create<Person>();
            roleType = this.fixture.M.Person.IsStudent;
            return person;
        }

        private IRenderedComponent<BootstrapCheckboxRole> Render(Person person, Allors.Workspace.Meta.IRoleType roleType) =>
            this.RenderComponent<BootstrapCheckboxRole>(parameters => parameters
                .AddCascadingValue(new EditContext(person))
                .Add(p => p.Object, person)
                .Add(p => p.RoleType, roleType));
    }
}
