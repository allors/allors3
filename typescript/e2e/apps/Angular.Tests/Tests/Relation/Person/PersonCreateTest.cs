// <copyright file="PersonCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.person.list;

namespace Tests.PersonTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PersonCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        public PersonCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var before = new People(this.Transaction).Extent().ToArray();

            var personCreate = this.people.CreatePerson();

            personCreate
                .Salutation.Select(new Salutations(this.Transaction).Mr)
                .FirstName.Set("Jos")
                .MiddleName.Set("de")
                .LastName.Set("Smos")
                .Function.Set("CEO")
                .Gender.Select(new GenderTypes(this.Transaction).Male)
                .Locale.Select(this.Transaction.GetSingleton().AdditionalLocales.FirstOrDefault())
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new People(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var person = after.Except(before).First();

            Assert.Equal(new Salutations(this.Transaction).Mr, person.Salutation);
            Assert.Equal("Jos", person.FirstName);
            Assert.Equal("de", person.MiddleName);
            Assert.Equal("Smos", person.LastName);
            Assert.Equal("CEO", person.Function);
            Assert.Equal(new GenderTypes(this.Transaction).Male, person.Gender);
            Assert.Equal(this.Transaction.GetSingleton().AdditionalLocales.FirstOrDefault(), person.Locale);
        }
    }
}
