// <copyright file="Medias.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class Medias
    {
        /// <summary>
        /// Replaces every <see cref="Media"/>'s <see cref="InlineMediaContent"/> (bytes in the database) with a
        /// <see cref="FileMediaContent"/> (bytes on the file store), preserving Type and Data. Media already
        /// backed by a file are skipped (idempotent). Returns the number converted.
        /// </summary>
        public static int ConvertInlineMediaContentToFile(ITransaction transaction)
        {
            var converted = 0;

            // Materialize the extent before mutating (building/deleting content changes transaction state).
            foreach (var media in transaction.Extent<Media>().ToArray())
            {
                if (media.MediaContent is InlineMediaContent inline)
                {
                    // Write-once replacement, forced to FileMediaContent (see MediaRule for the inline path).
                    var file = new FileMediaContentBuilder(transaction).Build();
                    file.Type = inline.Type;
                    file.Data = inline.Data;

                    media.MediaContent = file;
                    inline.CascadingDelete();

                    converted++;
                }
            }

            return converted;
        }
    }
}
