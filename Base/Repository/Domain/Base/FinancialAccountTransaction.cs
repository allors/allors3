// <copyright file="FinancialAccountTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("5500cb42-1aae-4816-9bc1-d63ff273f144")]
    #endregion
    public partial interface FinancialAccountTransaction : Object
    {
        #region Allors
        [Id("04411b65-a0a1-4e2c-9d10-a0ecfcf6c3d2")]
        #endregion
        [Size(-1)]

        string Description { get; set; }

        #region Allors
        [Id("07b3745c-581c-476b-a4a9-beacaa3bd700")]
        #endregion

        DateTime EntryDate { get; set; }

        #region Allors
        [Id("8f777804-597a-4604-a553-251e2e9d6502")]
        #endregion
        [Required]

        DateTime TransactionDate { get; set; }
    }
}
