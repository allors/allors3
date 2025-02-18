// <copyright file="SetupConfigurationTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
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
    }
}
