// <copyright file="FilterTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections;
    using System.Linq.Expressions;
    using Derivations;
    using Meta;
    using Xunit;

    public class ExpressionExtensionsTests
    {
        public ExpressionExtensionsTests()
        {
            var metaBuilder = new MetaBuilder();
            this.M = metaBuilder.Build();
        }

        private MetaPopulation M { get; }

        [Fact]
        public void Association()
        {
            Expression<Func<Person, IPropertyType>> expression = v => v.OrganisationWhereEmployee;

            var properties = expression.ToPropertyTypes(this.M);

            Assert.Single((IEnumerable)properties);
            Assert.Contains(this.M.Person.OrganisationWhereEmployee, properties);
        }

        [Fact]
        public void AssociationRole()
        {
            Expression<Func<Person, IPropertyType>> expression = v => v.OrganisationWhereEmployee.Organisation.Information;

            var properties = expression.ToPropertyTypes(this.M);

            Assert.Equal(2, properties.Length);
            Assert.Equal(this.M.Person.OrganisationWhereEmployee, properties[0]);
            Assert.Equal(this.M.Organisation.Information, properties[1]);
        }

        [Fact]
        public void Role()
        {
            Expression<Func<Organisation, IPropertyType>> expression = v => v.Name;

            var properties = expression.ToPropertyTypes(this.M);

            Assert.Single((IEnumerable)properties);
            Assert.Contains(this.M.Organisation.Name, properties);
        }

        [Fact]
        public void RoleRole()
        {
            Expression<Func<Organisation, IPropertyType>> expression = v => v.Employees.Person.FirstName;

            var properties = expression.ToPropertyTypes(this.M);

            Assert.Equal(2, properties.Length);
            Assert.Equal(this.M.Organisation.Employees, properties[0]);
            Assert.Equal(this.M.Person.FirstName, properties[1]);
        }
    }
}
