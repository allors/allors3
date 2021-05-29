// <copyright file="AgreementTermExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class AgreementTermExtensions
    {
        public static void AppsOnPostDerive(this AgreementTerm @this, ObjectOnPostDerive method)
        {
            var m = @this.DatabaseServices().M;
            method.Derivation.Validation.AssertAtLeastOne(@this, m.AgreementTerm.TermType, m.AgreementTerm.Description);
        }
    }
}
