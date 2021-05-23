// <copyright file="FinancialAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("27b45d45-459a-43cb-87b0-f8842ec56445")]
    #endregion
    public partial interface FinancialAccount : Object
    {
        #region Allors
        [Id("f90475c7-4a2d-42fd-bafd-96557c217c19")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        FinancialAccountTransaction[] FinancialAccountTransactions { get; set; }
    }
}
