// <copyright file="OpenItem.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("ec5b4a4d-f848-4ed2-8220-aa3224c345cf")]
    #endregion
    public partial class OpenItem : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("32f4dad8-a40b-4855-8356-1af8ac4bd976")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("cb975c75-3fd0-4907-92ea-0be2774cfe12")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Derived]
        [Workspace]
        public Party Party { get; set; }

        #region Allors
        [Id("7c164eea-45d1-4dba-afdb-1a66ae5df817")]
        #endregion
        [Indexed]
        [Required]
        [Workspace]
        public int SubAccountNumber { get; set; }

        #region Allors
        [Id("b2527a9c-fe81-4572-b6ec-2f4c55feba2b")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal Amount { get; set; }

        #region Allors
        [Id("e287e828-4673-40a2-bb52-67113c2c9068")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public PaymentMethod PaymentMethod { get; set; }

        #region Allors
        [Id("721f91bb-3e17-4e99-8a73-8c029962e726")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Invoice Invoice { get; set; }

        #region Allors
        [Id("b20abd90-c1b9-47a0-a877-9a97812b43eb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("495a3ca7-cff9-4d8c-8887-fa823de6df5b")]
        #endregion
        [Required]
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("bc346c06-3d48-487a-9aa5-c72cd5abf268")]
        #endregion
        public string TransactionNumber { get; set; }

        #region Allors
        [Id("b254656b-712a-43e9-a80d-30ea7d1ff222")]
        #endregion
        public string ReconciliationTransactionNumber { get; set; }

        #region Allors
        [Id("13dd1a92-8cb6-4899-8225-782b8213ca47")]
        #endregion
        [Required]
        public DateTime EntryDate { get; set; }

        #region Allors
        [Id("a82696ac-6ac7-4e63-a665-b36b32bcaf24")]
        #endregion
        [Required]
        public DateTime TransactionDate { get; set; }

        #region Allors
        [Id("d27dea46-bb5f-4542-8de2-3475249d88be")]
        #endregion
        [Required]
        public DateTime DueDate { get; set; }

        #region Allors
        [Id("4b9115c0-803a-450a-8552-e05567c3992a")]
        #endregion
        [Workspace]
        public bool BlockedForDunning { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion
    }
}
