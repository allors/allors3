// <copyright file="SetupConfigurationTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.TestPopulation;
    using Xunit;

    public class SetupConfigurationTests : DomainTest, IClassFixture<Fixture>
    {
        public SetupConfigurationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WhenPopulateWithAccountingHasOccuredDutchOrganisationsExist()
        {
            var internalOrganisations = new InternalOrganisations(this.Transaction).Extent().ToArray();
            Assert.Contains(internalOrganisations.Select(v => v.DisplayName).ToArray(), v => v == "dutchInternalOrganisation");
            Assert.Contains(internalOrganisations.Select(v => v.DisplayName).ToArray(), v => v == "Ned. Belastingdienst");
            Assert.Contains(internalOrganisations.Select(v => v.DisplayName).ToArray(), v => v == "Federale OverheidsDienst FINANCIËN");
        }

        [Fact]
        public void OrganisationDefaultsGenerateUniqueNames()
        {
            const int countPerMethod = 500;
            var faker = this.Transaction.Faker();
            var names = new List<string>(countPerMethod * 3);

            for (var i = 0; i < countPerMethod; i++)
            {
                names.Add(new OrganisationBuilder(this.Transaction).WithDefaults().Build().Name);
                names.Add(new OrganisationBuilder(this.Transaction).WithManufacturerDefaults(faker).Build().Name);
                names.Add(new OrganisationBuilder(this.Transaction).WithInternalOrganisationDefaults().Build().Name);
            }

            Assert.Equal(names.Count, names.Distinct().Count());
        }
    }
}
