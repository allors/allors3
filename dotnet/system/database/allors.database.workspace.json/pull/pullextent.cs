// <copyright file="PullExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Domain;
    using Meta;
    using Security;
    using Services;

    public class PullExtent
    {
        private readonly ITransaction transaction;
        private readonly Pull pull;
        private readonly IAccessControl acls;
        private readonly IPreparedSelects preparedSelects;
        private readonly IPreparedExtents preparedExtents;
        private readonly IPrefetchPolicyCache prefetchPolicyCache;

        public PullExtent(ITransaction transaction, Pull pull, IAccessControl acls, IPreparedSelects preparedSelects, IPreparedExtents preparedExtents, IPrefetchPolicyCache prefetchPolicyCache)
        {
            this.transaction = transaction;
            this.pull = pull;
            this.acls = acls;
            this.preparedExtents = preparedExtents;
            this.prefetchPolicyCache = prefetchPolicyCache;
            this.preparedSelects = preparedSelects;
        }

        public void Execute(PullResponseBuilder response)
        {
            if (this.pull.Extent == null && !this.pull.ExtentRef.HasValue)
            {
                throw new Exception("Either an Extent or an ExtentRef is required.");
            }

            var extent = this.pull.Extent ?? this.preparedExtents.Get(this.pull.ExtentRef.Value);

            if (this.pull.Results == null || this.pull.Results.Length == 0)
            {
                this.WithoutResults(extent, response);
            }
            else
            {
                this.WithResults(extent, response);
            }
        }

        private void WithoutResults(IExtent extent, PullResponseBuilder response)
        {
            var objects = extent.Build(this.transaction, this.pull.Arguments).ToArray();
            this.transaction.Prefetch(this.prefetchPolicyCache.Security, objects);

            var trimmed = objects.Where(response.Include).ToArray();

            var name = extent.ObjectType.PluralName;
            response.AddCollection(name, extent.ObjectType, trimmed);
        }

        private void WithResults(IExtent dataExtent, PullResponseBuilder response)
        {
            var results = this.pull.Results;

            var extent = dataExtent.Build(this.transaction, this.pull.Arguments);
            this.transaction.Prefetch(this.prefetchPolicyCache.Security, extent);

            IObject[] trimmed;
            if (results.All(v => v.Take.HasValue))
            {
                var resultsRequired = results.Aggregate(0, (acc, v) =>
                {
                    var resultRequired = v.Skip.HasValue ? v.Take.Value + v.Skip.Value : v.Take.Value;
                    return resultRequired > acc ? resultRequired : acc;
                });

                trimmed = extent.Where(response.Include).Take(resultsRequired).ToArray();
            }
            else
            {
                trimmed = extent.Where(response.Include).ToArray();
            }

            foreach (var result in results)
            {
                var name = result.Name;

                var objects = trimmed;
                if (result.Skip.HasValue || result.Take.HasValue)
                {
                    var paged = result.Skip.HasValue ? objects.Skip(result.Skip.Value) : objects;
                    if (result.Take.HasValue)
                    {
                        paged = paged.Take(result.Take.Value);
                    }

                    objects = paged.ToArray();
                }

                var select = result.Select;
                if (select == null && result.SelectRef.HasValue)
                {
                    select = this.preparedSelects.Get(result.SelectRef.Value);
                }

                if (select != null)
                {
                    var include = select.End.Include;

                    if (select.PropertyType != null)
                    {
                        objects = select.IsOne ?
                                      objects.Select(v => select.Get(v, this.acls)).Where(v => v != null).Cast<IObject>().Distinct().ToArray() :
                                      objects.SelectMany(v =>
                                      {
                                          var stepResult = select.Get(v, this.acls);
                                          return (IEnumerable<IObject>)stepResult ?? Array.Empty<IObject>();
                                      }).Distinct().ToArray();

                        var propertyType = select.End.PropertyType;
                        name ??= propertyType.PluralName;
                    }

                    name ??= dataExtent.ObjectType.PluralName;
                    response.AddCollection(name, (IComposite)select.GetObjectType() ?? dataExtent.ObjectType, objects, include);
                }
                else
                {
                    name ??= dataExtent.ObjectType.PluralName;
                    var include = result.Include;
                    response.AddCollection(name, dataExtent.ObjectType, objects, include);
                }
            }
        }
    }
}
