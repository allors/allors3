// <copyright file="DelegatedAccessObjectDelegateAccess.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public abstract partial class DelegatedAccessObjectDelegateAccess
    {
        public SecurityToken[] SecurityTokens { get; set; }

        public Revocation[] Revocations { get; set; }
    }
}
