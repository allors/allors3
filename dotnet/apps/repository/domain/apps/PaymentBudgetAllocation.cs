// <copyright file="PaymentBudgetAllocation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("2e588028-5de2-411c-ab43-b406ca735d5b")]
    #endregion
    public partial class PaymentBudgetAllocation : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("28f23032-b81c-4dbb-aa6c-24740ae3bb26")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Payment Payment { get; set; }

        #region Allors
        [Id("3245613c-71d3-4a5e-a687-6f5ac306d9df")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public BudgetItem BudgetItem { get; set; }

        #region Allors
        [Id("a982dfa7-4c81-41d6-93ec-ea380a526ad0")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}
