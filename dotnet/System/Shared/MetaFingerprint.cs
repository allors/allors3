// <copyright file="MetaFingerprint.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The fingerprint of a meta population: the hash of the tags of its composites, relation
    /// types and method types, sorted, so that the server and a client that generated its
    /// workspace meta from the same repository compute the same value, whatever the order of
    /// their collections. FNV-1a, 64 bit, over the UTF-8 bytes of each tag followed by a line
    /// feed, as sixteen lowercase hex characters; the TypeScript workspace computes the same.
    /// </summary>
    public static class MetaFingerprint
    {
        private const ulong OffsetBasis = 0xcbf29ce484222325;

        private const ulong Prime = 0x100000001b3;

        public static string Compute(IEnumerable<string> tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }

            var hash = OffsetBasis;

            foreach (var tag in tags.OrderBy(v => v, StringComparer.Ordinal))
            {
                foreach (var @byte in Encoding.UTF8.GetBytes(tag))
                {
                    hash = unchecked((hash ^ @byte) * Prime);
                }

                hash = unchecked((hash ^ (byte)'\n') * Prime);
            }

            return hash.ToString("x16");
        }
    }
}
