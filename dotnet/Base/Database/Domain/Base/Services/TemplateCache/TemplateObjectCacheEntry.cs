// <copyright file="PermissionsCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Allors.Document.OpenDocument;

    public class TemplateObjectCacheEntry
    {
        public TemplateObjectCacheEntry(Template template)
        {
            this.Revision = template.Media.Revision.Value;
            this.Object = template.TemplateType.IsOpenDocumentTemplate && template.Media.MediaContent?.Data is { } data ?
                new OpenDocumentTemplate(data, template.Arguments) :
                null;
        }

        public Guid Revision { get; }

        public object Object { get; }
    }
}
