// <copyright file="PaymentApplication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("6fef08f0-d4cb-42f4-a10f-fb31787f65c3")]
    #endregion
    public partial class PaymentApplication : Deletable, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("1147413e-9b57-45b3-a15c-44923e83001a")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal AmountApplied { get; set; }

        #region Allors
        [Id("b5f00552-5976-4368-9f38-dc4734b1c4af")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public InvoiceItem InvoiceItem { get; set; }

        #region Allors
        [Id("d2a02ce6-569d-41ae-b54d-4a2347b84835")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Invoice Invoice { get; set; }

        #region Allors
        [Id("2ece9423-3594-4d48-8341-d59472801a92")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public OrderItem OrderItem { get; set; }

        #region Allors
        [Id("75415ce0-a251-4a37-a1e4-165c20b9be20")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Order Order { get; set; }

        #region Allors
        [Id("deb07a2f-6344-4888-bd1a-97413e82700a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public BillingAccount BillingAccount { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
