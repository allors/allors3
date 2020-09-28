// <copyright file="ExternalAccountingTransaction.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("6bfa631c-80f4-495f-bb9a-0d3351390d64")]
    #endregion
    public partial interface ExternalAccountingTransaction : AccountingTransaction
    {
        #region Allors
        [Id("327fc2cb-9589-4e9d-b9e6-7429cbe14746")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        Party FromParty { get; set; }

        #region Allors
        [Id("681312d3-63cd-45a2-883c-4a907d379f52")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        Party ToParty { get; set; }
    }
}
