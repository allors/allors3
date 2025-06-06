// <copyright file="PostalCodes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PostalCodes
    {
        private Cache<string, PostalCode> postalCodeByCode;

        public Cache<string, PostalCode> PostalCodeByCode => this.postalCodeByCode ??= new Cache<string, PostalCode>(this.Transaction, this.M.PostalCode.Code);
    }
}
