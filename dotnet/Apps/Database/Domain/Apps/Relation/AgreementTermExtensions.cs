// <copyright file="AgreementTermExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public static partial class AgreementTermExtensions
    {
        public static void AppsOnPostDerive(this AgreementTerm @this, ObjectOnPostDerive method)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();
            method.Derivation.Validation.AssertAtLeastOne(@this, m.AgreementTerm.TermType, m.AgreementTerm.Description);
        }
    }
}
