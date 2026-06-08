// <copyright file="IMediaContentFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public interface IMediaContentFactory
    {
        /// <summary>
        /// Builds a new, empty <see cref="MediaContent"/> of the concrete type dictated by the host's
        /// storage policy, configured at startup (see the registration in DatabaseServices).
        /// </summary>
        MediaContent Create(ITransaction transaction);
    }
}
