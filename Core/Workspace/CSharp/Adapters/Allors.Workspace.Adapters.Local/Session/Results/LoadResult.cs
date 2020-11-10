// <copyright file="ILoadResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    
    public class LoadResult : Result, ILoadResult
    {
        public LoadResult(Workspace workspace)
        {
            this.Workspace = workspace;

            this.Objects = new Dictionary<string, IObject>();
            this.Collections = new Dictionary<string, IObject[]>();
            this.Values = new Dictionary<string, object>();
        }

        IReadOnlyDictionary<string, IObject> ILoadResult.Objects => this.Objects;

        IReadOnlyDictionary<string, IObject[]> ILoadResult.Collections => this.Collections;

        IReadOnlyDictionary<string, object> ILoadResult.Values => this.Values;

        public Dictionary<string, IObject> Objects { get; }

        public Dictionary<string, IObject[]> Collections { get; }

        public Dictionary<string, object> Values { get; }

        private IWorkspace Workspace { get; }

        public T[] GetCollection<T>()
        {
            var objectType = this.Workspace.ObjectFactory.GetObjectType<T>();
            var key = objectType.PluralName;
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) => this.Collections.TryGetValue(key, out var collection) ? collection?.Cast<T>().ToArray() : null;

        public T GetObject<T>()
            where T : class, IObject
        {
            var objectType = this.Workspace.ObjectFactory.GetObjectType<T>();
            var key = objectType.SingularName;
            return this.GetObject<T>(key);
        }

        public T GetObject<T>(string key)
            where T : class, IObject => this.Objects.TryGetValue(key, out var @object) ? (T)@object : null;

        public object GetValue(string key) => this.Values[key];
    }
}
