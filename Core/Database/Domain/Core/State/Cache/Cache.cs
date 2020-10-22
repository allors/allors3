// <copyright file="Cache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;

    using Allors;
    using Allors.Meta;

    public class Cache<TKey, TObject>
        where TObject : class, IObject
    {
        private IDictionary<TKey, long> cache;

        public Cache(ISession session, RoleType roleType)
        {
            if (!roleType.ObjectType.IsUnit)
            {
                throw new ArgumentException("ObjectType of RoleType should be a Unit");
            }

            this.Session = session;
            this.RoleType = roleType;
        }

        public ISession Session { get; }

        public RoleType RoleType { get; }

        public TObject this[TKey key]
        {
            get
            {
                if (this.cache == null)
                {
                    this.cache = this.Session.GetCache<TKey>(typeof(TObject), this.RoleType);
                }

                if (!this.cache.TryGetValue(key, out var objectId))
                {
                    var extent = this.Session.Extent<TObject>();
                    extent.Filter.AddEquals(this.RoleType, key);

                    var @object = extent.First;
                    if (@object != null)
                    {
                        objectId = @object.Id;
                        if (!@object.Strategy.IsNewInSession)
                        {
                            this.cache[key] = @object.Id;
                        }
                    }
                }

                return (TObject)this.Session.Instantiate(objectId);
            }
        }

        public CacheMerger<TKey, TObject> Merger() => new CacheMerger<TKey, TObject>(this);

        public class CacheMerger<TKey, TObject>
            where TObject : class, IObject
        {
            private readonly Cache<TKey, TObject> cache;
            private readonly ISession session;
            private readonly IClass @class;
            private readonly IRoleType roleType;

            internal CacheMerger(Cache<TKey, TObject> cache)
            {
                this.cache = cache;
                this.session = cache.Session;
                this.@class = (IClass)this.session.Database.ObjectFactory.GetObjectTypeForType(typeof(TObject));
                this.roleType = this.cache.RoleType;
            }

            public Func<TKey, Action<TObject>, TObject> Action() =>
                (id, action) =>
                {
                    var @object = this.cache[id] ?? (TObject)Allors.ObjectBuilder.Build(this.session, this.@class);

                    @object.Strategy.SetUnitRole(this.roleType, id);
                    action(@object);

                    return @object;
                };
        }
    }
}
