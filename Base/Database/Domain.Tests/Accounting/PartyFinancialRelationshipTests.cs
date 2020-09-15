// <copyright file="PartyFinancialRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using System.Linq;
    using Allors.Domain.TestPopulation;
    using Xunit;

    public class PartyFinancialRelationshipTests : DomainTest
    {
        [Fact]
        public void DeriveOpenOrderAmountOnNewSalesOrder()
        {
            var order = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();

            this.Session.Derive();

            var partyFinancial = order.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.First(v => v.InternalOrganisation == order.TakenBy);

            Assert.True(partyFinancial.OpenOrderAmount == order.TotalIncVat);
        }
    }
}
