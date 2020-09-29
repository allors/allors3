// <copyright file="AmountDue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("848053ee-18d8-4962-81c3-bd6c7837565a")]
    #endregion
    public partial class AmountDue : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0274d4d3-3f07-408c-89e3-f5367acd5fab")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

        #region Allors
        [Id("3856c988-32d3-455d-89d8-aa1eaa80dcce")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("39d2f4f2-0c16-40f5-990b-38bad15fae99")]
        #endregion
        public DateTime TransactionDate { get; set; }

        #region Allors
        [Id("3ca978b2-8c0a-4fec-8b98-88e9ea3b2966")]
        #endregion
        public DateTime BlockedForDunning { get; set; }

        #region Allors
        [Id("43193cac-15ad-4a1a-8945-f4ecb7d93291")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal AmountVat { get; set; }

        #region Allors
        [Id("5cb888fd-bcff-4eef-8ad6-efab3434364d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public BankAccount BankAccount { get; set; }

        #region Allors
        [Id("90b4eaea-21cd-4a04-a64b-3c3dce0718d9")]
        #endregion
        public DateTime ReconciliationDate { get; set; }

        #region Allors
        [Id("953877d2-055c-4274-afa5-91fd425b5449")]
        #endregion
        [Size(256)]
        public string InvoiceNumber { get; set; }

        #region Allors
        [Id("98ec45be-fea4-4df7-91fb-6643edf74784")]
        #endregion
        public int DunningStep { get; set; }

        #region Allors
        [Id("a40ae239-df13-47e1-8fa2-cfcb4946b966")]
        #endregion
        public int SubAccountNumber { get; set; }

        #region Allors
        [Id("a6693763-e246-4ae8-bd37-74313d32883b")]
        #endregion
        [Size(256)]
        public string TransactionNumber { get; set; }

        #region Allors
        [Id("acedb9ed-b0de-464f-86ec-621022938ad7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public DebitCreditConstant Side { get; set; }

        #region Allors
        [Id("b0570264-3211-4444-a69f-1cdb2eb6e783")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Currency Currency { get; set; }

        #region Allors
        [Id("d227669f-8052-4962-8ccf-a775355691f1")]
        #endregion
        public bool BlockedForPayment { get; set; }

        #region Allors
        [Id("def3c00e-f065-48e5-97a2-22497f1800b3")]
        #endregion
        public DateTime DateLastReminder { get; set; }

        #region Allors
        [Id("e9b2fc3f-c6ed-4e67-a634-9ee78f824ad8")]
        #endregion
        [Size(256)]
        public string YourReference { get; set; }

        #region Allors
        [Id("edad9c25-ef5e-4326-aba6-535deb6a8a7e")]
        #endregion
        [Size(256)]
        public string OurReference { get; set; }

        #region Allors
        [Id("f027183b-d8a1-4909-bedc-5d16a62d6bc2")]
        #endregion
        [Size(256)]
        public string ReconciliationNumber { get; set; }

        #region Allors
        [Id("f18c665b-4f88-4e97-950c-08a38d9f0d93")]
        #endregion
        public DateTime DueDate { get; set; }

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
