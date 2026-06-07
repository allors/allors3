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
        /// Replaces every <see cref="Media"/>'s <see cref="EmbeddedMediaContent"/> (bytes in the database) with an
        /// <see cref="ExternalMediaContent"/> (bytes in external storage), preserving Type and Data. Media already
        /// stored externally are skipped (idempotent). Returns the number converted.
        /// </summary>
        public static int ConvertEmbeddedMediaContentToExternal(ITransaction transaction)
        {
            var converted = 0;

            // Materialize the extent before mutating (building/deleting content changes transaction state).
            foreach (var media in transaction.Extent<Media>().ToArray())
            {
                if (media.MediaContent is EmbeddedMediaContent embedded)
                {
                    // Write-once replacement, forced to ExternalMediaContent (see MediaRule for the default path).
                    var external = new ExternalMediaContentBuilder(transaction).Build();
                    external.Type = embedded.Type;
                    external.Data = embedded.Data;

                    media.MediaContent = external;
                    embedded.CascadingDelete();

                    converted++;
                }
            }

            return converted;
        }
    }
}
