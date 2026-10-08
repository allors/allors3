// <copyright file="MetaPopulationExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Linq;
    using Meta;

    public static class MetaPopulationExtensions
    {
        /// <summary>
        /// The fingerprint of the workspace meta: <see cref="MetaFingerprint"/> over the tags of
        /// its composites, relation types and method types, the same members the server hashes
        /// for the workspace the meta was generated for.
        /// </summary>
        public static string Fingerprint(this IMetaPopulation @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return MetaFingerprint.Compute(@this.Composites.Select(v => v.Tag)
                .Concat(@this.RelationTypes.Select(v => v.Tag))
                .Concat(@this.MethodTypes.Select(v => v.Tag)));
        }
    }
}
