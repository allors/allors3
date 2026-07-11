// <copyright file="BootstrapCheckboxGroupRoleTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Blazor.Bootstrap.Tests
{
    using Allors.Workspace.Blazor.Bootstrap.Forms.Roles;
    using Bunit;
    using Microsoft.AspNetCore.Components.Forms;
    using Xunit;
    using Organisation = Allors.Workspace.Domain.Organisation;
    using Person = Allors.Workspace.Domain.Person;

    public class BootstrapCheckboxGroupRoleTests : BootstrapRoleTestContext, IClassFixture<WorkspaceFixture>
    {
        private readonly WorkspaceFixture fixture;

        public BootstrapCheckboxGroupRoleTests(WorkspaceFixture fixture) => this.fixture = fixture;

        [Fact]
        public void RendersOnlySelectedOptionsAsChecked()
        {
            var cut = this.Render(out var person, out var org1, out var org2);

            var inputs = cut.FindAll("input");
            Assert.Equal(2, inputs.Count);
            Assert.True(inputs[0].HasAttribute("checked"));   // org1 is in the collection
            Assert.False(inputs[1].HasAttribute("checked"));  // org2 is not
        }

        [Fact]
        public void ClickingAnOptionAddsItToTheCollection()
        {
            var cut = this.Render(out var person, out var org1, out var org2);

            cut.FindAll("input")[1].Click();

            Assert.Contains(org2, person.CycleMany);
        }

        [Fact]
        public void ClickingASelectedOptionRemovesItFromTheCollection()
        {
            var cut = this.Render(out var person, out var org1, out var org2);

            cut.FindAll("input")[0].Click();

            Assert.DoesNotContain(org1, person.CycleMany);
        }

        private IRenderedComponent<BootstrapCheckboxGroupRole> Render(out Person person, out Organisation org1, out Organisation org2)
        {
            var session = this.fixture.Workspace.CreateSession();
            person = session.Create<Person>();
            org1 = session.Create<Organisation>();
            org2 = session.Create<Organisation>();
            person.AddCycleMany(org1);

            var roleType = this.fixture.M.Person.CycleMany;
            var options = new IObject[] { org1, org2 };

            var model = person;
            return this.RenderComponent<BootstrapCheckboxGroupRole>(parameters => parameters
                .AddCascadingValue(new EditContext(model))
                .Add(p => p.Object, model)
                .Add(p => p.RoleType, roleType)
                .Add(p => p.Options, options));
        }
    }
}
