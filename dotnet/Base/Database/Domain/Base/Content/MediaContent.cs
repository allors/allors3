// <copyright file="MediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial interface MediaContent
    {
        // The bytes of this content. The storage strategy is implementation specific:
        // EmbeddedMediaContent keeps them in the database, ExternalMediaContent in external storage.
        byte[] Data { get; set; }

        // True when this content has non-empty bytes. Cheap to evaluate (no full read for external storage).
        bool HasData { get; }
    }
}
