// <copyright file="ExternalMediaContents.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class ExternalMediaContents
    {
        /// <summary>
        /// Reconciles the file store against the database: deletes every file that no live
        /// <see cref="ExternalMediaContent"/> owns — orphans left by a rolled-back write or a deferred delete.
        /// Must run single-user; it is invoked from <see cref="Upgrade"/>, where the load process is the only
        /// connection, so a file with no live owner is always a true orphan and never an in-flight write.
        /// Returns the number removed.
        /// </summary>
        public static int ReconcileFiles(ITransaction transaction)
        {
            var liveIds = new HashSet<long>();
            foreach (ExternalMediaContent content in transaction.Extent<ExternalMediaContent>())
            {
                liveIds.Add(content.Id);
            }

            var storage = transaction.Database.Services.Get<IMediaContentStorage>();

            var removed = 0;
            // Materialize before deleting: mutating the directory while enumerating it can skip entries.
            foreach (var id in storage.Enumerate().ToList())
            {
                if (!liveIds.Contains(id))
                {
                    storage.Delete(id);
                    removed++;
                }
            }

            return removed;
        }
    }
}
