// <copyright file="AccountingTransactionDetail.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("e41be1b2-715b-4bc0-b095-ac23d9950ee4")]
    #endregion
    public partial class AccountingTransactionDetail : DelegatedAccessObject
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Object DelegatedAccess { get; set; }


        #endregion

        #region Allors
        [Id("63447735-fdfc-4f32-ab89-d848903d71eb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public AccountingTransactionDetail AssociatedWith { get; set; }

        #region Allors
        [Id("644b956b-58e3-465b-b431-5637d9a209e5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace]
        public OrganisationGlAccount OrganisationGlAccount { get; set; }

        #region Allors
        [Id("9b5a3978-9859-432a-939b-73838c2bb3b2")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal Amount { get; set; }

        #region Allors
        [Id("4a3c1ca1-049f-4c41-aa8d-44a2d02ff09e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public BalanceSide BalanceSide { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }



        #endregion
    }
}
