// <copyright file="FileMediaContents.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public partial class FileMediaContents
    {
        /// <summary>
        /// Removes orphaned file-backed media: files whose id is below the highest live
        /// <see cref="FileMediaContent"/> id ("ceiling") and that no live content owns (left by a rolled-back
        /// write or a deferred delete). Files at or above the ceiling are left alone — their content may belong
        /// to a not-yet-committed transaction (ids are allocated before commit). Returns the number removed.
        /// </summary>
        public static int RemoveOrphanedFiles(ITransaction transaction)
        {
            var liveIds = new HashSet<long>();
            var ceiling = 0L;
            foreach (FileMediaContent content in transaction.Extent<FileMediaContent>())
            {
                liveIds.Add(content.Id);
                if (content.Id > ceiling)
                {
                    ceiling = content.Id;
                }
            }

            var storage = transaction.Database.Services.Get<IMediaContentStorage>();

            var removed = 0;
            foreach (var id in storage.Enumerate())
            {
                if (id < ceiling && !liveIds.Contains(id))
                {
                    storage.Delete(id);
                    removed++;
                }
            }

            return removed;
        }
    }
}
