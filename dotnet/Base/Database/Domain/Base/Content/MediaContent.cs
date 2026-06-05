// <copyright file="MediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial interface MediaContent
    {
        // The bytes of this content. The storage strategy is implementation specific:
        // InlineMediaContent keeps them in the database, FileMediaContent on the filesystem.
        byte[] Data { get; set; }
    }
}
