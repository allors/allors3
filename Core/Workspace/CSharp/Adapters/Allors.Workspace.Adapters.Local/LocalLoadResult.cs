// <copyright file="LocalLoadResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System.Collections.Generic;
    using System.Linq;

    public class LocalLoadResult : ILoadResult
    {
        public LocalLoadResult(LocalSession session, LocalPullResult pullResult)
        {
            this.Workspace = session.Workspace;

            this.Collections = pullResult.CollectionsByName.ToDictionary(v => v.Key, v => v.Value.Select(w => session.Instantiate(w.Id)).ToArray());
            this.Objects = pullResult.ObjectByName.ToDictionary(v => v.Key, v => session.Instantiate(v.Value.Id));
            this.Values = pullResult.ValueByName;
        }

        public bool HasErrors { get; }
        public string ErrorMessage { get; }
        public string[] VersionErrors { get; }
        public string[] AccessErrors { get; }
        public string[] MissingErrors { get; }
        public IDerivationError[] DerivationErrors { get; }

        IReadOnlyDictionary<string, IObject[]> ILoadResult.Collections => this.Collections;
        public Dictionary<string, IObject[]> Collections { get; }
        IReadOnlyDictionary<string, IObject> ILoadResult.Objects => this.Objects;
        public Dictionary<string, IObject> Objects { get; }
        IReadOnlyDictionary<string, object> ILoadResult.Values => this.Values;
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
