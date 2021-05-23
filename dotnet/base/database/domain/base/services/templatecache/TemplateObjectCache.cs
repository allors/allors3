// <copyright file="PermissionsCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Concurrent;
    using Database;

    public class TemplateObjectCache : ITemplateObjectCache
    {
        private readonly IDatabase database;

        private readonly ConcurrentDictionary<long, TemplateObjectCacheEntry> templateObjectCacheByTemplateId;

        public TemplateObjectCache() => this.templateObjectCacheByTemplateId = new ConcurrentDictionary<long, TemplateObjectCacheEntry>();

        public object Get(Template template)
        {
            this.templateObjectCacheByTemplateId.TryGetValue(template.Id, out var templateObjectCachEntry);
            if (templateObjectCachEntry?.Revision != template.Media.Revision)
            {
                templateObjectCachEntry = new TemplateObjectCacheEntry(template);
                this.templateObjectCacheByTemplateId[template.Id] = templateObjectCachEntry;
            }

            return templateObjectCachEntry.Object;
        }
    }
}
