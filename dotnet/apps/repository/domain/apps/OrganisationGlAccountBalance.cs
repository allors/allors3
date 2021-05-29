// <copyright file="OrganisationGlAccountBalance.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("67a8352d-7fe0-4398-93c3-50ec8d3e8038")]
    #endregion
    public partial class OrganisationGlAccountBalance : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

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
