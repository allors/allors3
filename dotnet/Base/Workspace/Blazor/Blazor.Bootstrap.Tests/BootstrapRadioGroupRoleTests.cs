// <copyright file="BootstrapRadioGroupRoleTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Blazor.Bootstrap.Tests
{
    using System.Globalization;
    using Allors.Workspace.Blazor.Bootstrap.Forms.Roles;
    using Allors.Workspace.Meta;
    using Bunit;
    using Microsoft.AspNetCore.Components.Forms;
    using Xunit;
    using Gender = Allors.Workspace.Domain.Gender;
    using Organisation = Allors.Workspace.Domain.Organisation;
    using Person = Allors.Workspace.Domain.Person;

    public class BootstrapRadioGroupRoleTests : BootstrapRoleTestContext, IClassFixture<WorkspaceFixture>
    {
        private readonly WorkspaceFixture fixture;

        public BootstrapRadioGroupRoleTests(WorkspaceFixture fixture) => this.fixture = fixture;

        [Fact]
        public void RendersOnlyTheSelectedOptionAsChecked()
        {
            var cut = this.RenderCycleOne(out _, out _, out _);

            var inputs = cut.FindAll("input");
            Assert.Equal(2, inputs.Count);
            Assert.True(inputs[0].HasAttribute("checked"));   // org1 is the role
            Assert.False(inputs[1].HasAttribute("checked"));  // org2 is not
        }

        [Fact]
        public void ClickingAnOptionSetsTheRole()
        {
            var cut = this.RenderCycleOne(out var person, out _, out var org2);

            cut.FindAll("input")[1].Click();

            Assert.Equal(org2, person.CycleOne);
        }

        /// <summary>
        /// The browser groups radios by their name attribute, so the name has to identify the role
        /// field. Without one it is derived from the bind target, which is RoleField.Model for
        /// every role field alike.
        /// </summary>
        [Fact]
        public void GroupsTheOptionsUnderTheFieldName()
        {
            var cut = this.RenderCycleOne(out var person, out _, out _);

            var expected = "CycleOne_" + person.Id.ToString(CultureInfo.InvariantCulture).Replace('-', '_');
            Assert.All(cut.FindAll("input"), input => Assert.Equal(expected, input.GetAttribute("name")));
        }

        [Fact]
        public void GroupsForDifferentRolesDoNotShareAName()
        {
            var session = this.fixture.Workspace.CreateSession();
            var person = session.Create<Person>();
            var organisation = session.Create<Organisation>();
            var gender = session.Create<Gender>();

            var cycleOne = this.Render(person, this.fixture.M.Person.CycleOne, new IObject[] { organisation });
            var genders = this.Render(person, this.fixture.M.Person.Gender, new IObject[] { gender });

            Assert.NotEqual(
                cycleOne.Find("input").GetAttribute("name"),
                genders.Find("input").GetAttribute("name"));
        }

        [Fact]
        public void GroupsForDifferentObjectsDoNotShareAName()
        {
            var session = this.fixture.Workspace.CreateSession();
            var organisation = session.Create<Organisation>();
            var roleType = this.fixture.M.Person.CycleOne;
            var options = new IObject[] { organisation };

            var first = this.Render(session.Create<Person>(), roleType, options);
            var second = this.Render(session.Create<Person>(), roleType, options);

            Assert.NotEqual(
                first.Find("input").GetAttribute("name"),
                second.Find("input").GetAttribute("name"));
        }

        private IRenderedComponent<BootstrapRadioGroupRole> RenderCycleOne(out Person person, out Organisation org1, out Organisation org2)
        {
            var session = this.fixture.Workspace.CreateSession();
            person = session.Create<Person>();
            org1 = session.Create<Organisation>();
            org2 = session.Create<Organisation>();
            person.CycleOne = org1;

            return this.Render(person, this.fixture.M.Person.CycleOne, new IObject[] { org1, org2 });
        }

        private IRenderedComponent<BootstrapRadioGroupRole> Render(IObject @object, IRoleType roleType, IObject[] options) =>
            this.RenderComponent<BootstrapRadioGroupRole>(parameters => parameters
                .AddCascadingValue(new EditContext(@object))
                .Add(p => p.Object, @object)
                .Add(p => p.RoleType, roleType)
                .Add(p => p.Options, options));
    }
}
