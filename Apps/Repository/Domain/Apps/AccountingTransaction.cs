// <copyright file="AccountingTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("785a36a9-4710-4f3f-bd26-dbaff5353535")]
    #endregion
    public partial interface AccountingTransaction : Object
    {
        #region Allors
        [Id("4e4cb94c-424c-4824-ad44-5bb1c7312a52")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        AccountingTransactionDetail[] AccountingTransactionDetails { get; set; }

        #region Allors
        [Id("657f2688-4af0-4580-add2-c8a30b32e016")]
        #endregion
        [Required]
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("77910a3f-3547-4d6b-92e0-f1fc136e22da")]
        #endregion
        [Required]
        DateTime TransactionDate { get; set; }

        #region Allors
        [Id("a29cb739-8d2f-4e7d-a652-af8d2e190658")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal DerivedTotalAmount { get; set; }

        #region Allors
        [Id("a7fb7e5a-287a-41a1-b6b9-bd56601732f3")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        AccountingTransactionNumber AccountingTransactionNumber { get; set; }

        #region Allors
        [Id("be061dda-bb8f-4bc1-b386-dc0c05dc6eaf")]
        #endregion
        [Required]
        DateTime EntryDate { get; set; }
    }
}
