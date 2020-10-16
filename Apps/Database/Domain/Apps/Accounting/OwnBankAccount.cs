// <copyright file="OwnBankAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class OwnBankAccount
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistIsActive)
            {
                this.IsActive = true;
            }
        }

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (iteration.IsMarked(this) || changeSet.IsCreated(this) || changeSet.HasChangedRole(this, this.Meta.BankAccount))
            {
                if (this.ExistBankAccount)
                {
                    iteration.Mark(this.BankAccount);
                }
            }
        }

        public void AppsOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;

            if (this.ExistInternalOrganisationWhereActiveCollectionMethod && this.InternalOrganisationWhereActiveCollectionMethod.DoAccounting)
            {
                derivation.Validation.AssertAtLeastOne(this, this.M.Cash.GeneralLedgerAccount, this.M.Cash.Journal);
            }

            derivation.Validation.AssertExistsAtMostOne(this, this.M.Cash.GeneralLedgerAccount, this.M.Cash.Journal);
        }
    }
}
