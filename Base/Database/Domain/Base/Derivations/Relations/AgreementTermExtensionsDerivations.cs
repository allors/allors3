// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class AgreementTermExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdAgreementTermExtensions = changeSet.Created.Select(session.Instantiate).OfType<AgreementTerm>();

                foreach(var agreementTerm in createdAgreementTermExtensions)
                {
                    validation.AssertAtLeastOne(agreementTerm, M.AgreementTerm.TermType, M.AgreementTerm.Description);
                }
            }
        }

        public static void AgreementTermExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("97d8fcd3-e55c-42d9-ac22-98fc77a7367e")] = new AgreementTermExtensionsCreationDerivation();
        }
    }
}
