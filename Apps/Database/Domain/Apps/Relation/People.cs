// <copyright file="People.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class People
    {
        public static void Daily(ITransaction transaction)
        {
            var people = new People(transaction).Extent();

            foreach (Person person in people)
            {
                person.DeriveRelationships();
            }
        }

        protected override void AppsPrepare(Setup setup)
        {
            setup.AddDependency(this.Meta, this.M.Role);
            setup.AddDependency(this.Meta, this.M.PersonRole);
            setup.AddDependency(this.Meta, this.M.InternalOrganisation);
            setup.AddDependency(this.ObjectType, this.M.Locale);
            setup.AddDependency(this.ObjectType, this.M.ContactMechanismPurpose);
            setup.AddDependency(this.ObjectType, this.M.InternalOrganisation);
            setup.AddDependency(this.ObjectType, this.M.PersonalTitle);
        }

        protected override void AppsSetup(Setup setup)
        {
            var employeeUserGroup = new UserGroups(this.Transaction).Employees;
            var internalOrganisations = new Organisations(this.Transaction).InternalOrganisations();

            var people = new People(this.Transaction).Extent();

            var employeesByEmployer = new Employments(this.Transaction).Extent()
                .GroupBy(v => v.Employer)
                .ToDictionary(v => v.Key, v => new HashSet<Person>(v.Select(w => w.Employee).ToArray()));

            foreach (Person person in people)
            {
                foreach (var internalOrganisation in internalOrganisations)
                {
                    employeeUserGroup.AddMember(person);

                    if (employeesByEmployer.TryGetValue(internalOrganisation, out var employees))
                    {
                        if (employees.Contains(person))
                        {
                            break;
                        }
                    }

                    new EmploymentBuilder(this.Transaction).WithEmployer(internalOrganisation).WithEmployee(person).Build();
                }
            }
        }

        protected override void AppsSecure(Security config)
        {
            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };

            config.GrantOwner(this.ObjectType, full);
        }
    }
}
