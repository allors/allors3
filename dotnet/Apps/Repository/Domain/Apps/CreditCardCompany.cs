// <copyright file="CreditCardCompany.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("86d934de-a5cf-46d3-aad3-2626c43ebc85")]
    #endregion
    public partial class CreditCardCompany : UniquelyIdentifiable
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }
        #endregion

        #region Allors
        [Id("05860987-77be-4d8d-823d-99dd0e2cc822")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

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
