// <copyright file="ISecure.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public interface ISecure
    {
        void Prepare(Security setup);

        void Secure(Security security);
    }
}
