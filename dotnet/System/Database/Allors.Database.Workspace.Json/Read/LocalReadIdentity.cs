// <copyright file="LocalReadIdentity.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;

    /// <summary>
    /// A trusted Local host's population-bound user selection. Never construct this binding from
    /// an untrusted expected scope. Rebind after offline population replacement and authentication.
    /// </summary>
    public sealed class LocalReadIdentity
    {
        public LocalReadIdentity(Guid population, long userId)
        {
            if (population == Guid.Empty)
            {
                throw new ArgumentException("Bind the Local user to a known, nonempty population incarnation.", nameof(population));
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "Bind the Local identity to an existing committed user with a positive object ID.");
            }

            this.Population = population;
            this.UserId = userId;
        }

        public Guid Population { get; }

        public long UserId { get; }
    }
}
