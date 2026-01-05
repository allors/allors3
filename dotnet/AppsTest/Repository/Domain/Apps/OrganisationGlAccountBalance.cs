// <copyright file="OrganisationGlAccountBalance.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("67a8352d-7fe0-4398-93c3-50ec8d3e8038")]
    #endregion
    public partial class OrganisationGlAccountBalance : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("347426a0-8678-4eaa-9733-4bf719bad0c2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public OrganisationGlAccount OrganisationGlAccount { get; set; }

        #region Allors
        [Id("05b607bf-ab65-447b-b41c-c9e3ade2dc0e")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal OpeningBalance { get; set; }

        #region Allors
        [Id("b80d6897-3bcb-44b5-9625-d7d16ffb25a0")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal DebitAmount { get; set; }

        #region Allors
        [Id("40a12787-b0cc-41bb-a2bc-9cc4290f46bc")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal CreditAmount { get; set; }

        #region Allors
        [Id("94c5bafb-29ef-4268-846e-5fda5c62af5c")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

        #region Allors
        [Id("f7325700-87e9-4753-8b0b-de459a6926e7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public AccountingPeriod AccountingPeriod { get; set; }

        #region Allors
        [Id("dd741b96-53ac-4339-940c-87cd6a49a6db")]
        #endregion
        [Required]
        public Guid DerivationTrigger { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
