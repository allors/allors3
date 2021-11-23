// <copyright file="Security.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Security
    {
        public void Grantemployee(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.EmployeeId, objectType, operations);

        public void GrantCustomerContact(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.CustomerContactId, objectType, operations);

        private void CustomOnPreSetup()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var security = new Security(this.transaction);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                security.GrantAdministrator(@class, full);
                security.Grantemployee(@class, Operations.Read);
                security.GrantCreator(@class, full);

                if (@class.Equals(m.WorkTask) ||
                    @class.Equals(m.Locale) ||
                    @class.Equals(m.Currency) ||
                    @class.Equals(m.Catalogue) ||
                    @class.Equals(m.Scope) ||
                    @class.Equals(m.ProductCategory) ||
                    @class.Equals(m.SerialisedItemCharacteristicType) ||
                    @class.Equals(m.UnitOfMeasure) ||
                    @class.Equals(m.Brand) ||
                    @class.Equals(m.Model) ||
                    @class.Equals(m.UnifiedGood))
                {
                    security.GrantCustomerContact(@class, Operations.Read, Operations.Write);
                }
            }
        }

        private void CustomOnPostSetup()
        {
        }
    }
}
