// <copyright file="RemotePullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Pull;

    public class PullResult : Result, IPullResult
    {
        public PullResult(Adapters.Session session, PullResponse response) : base(session, response)
        {
            this.Workspace = session.Workspace;

            this.Objects = response.o.ToDictionary(
                pair => pair.Key,
                pair => session.GetOne<IObject>(pair.Value),
                StringComparer.OrdinalIgnoreCase);
            this.Collections = response.c.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(session.GetOne<IObject>).ToArray(),
                StringComparer.OrdinalIgnoreCase);
            this.Values = response.v.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        private IWorkspace Workspace { get; }

        public IDictionary<string, IObject> Objects { get; }

        public IDictionary<string, IObject[]> Collections { get; }

        public IDictionary<string, object> Values { get; }

        public T[] GetCollection<T>() where T : IObject
        {
            var objectType = this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.PluralName;
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) where T : IObject =>
            this.Collections.TryGetValue(key, out var collection) ? collection?.Cast<T>().ToArray() : null;

        public T GetObject<T>()
            where T : class, IObject
        {
            var objectType = this.Workspace.DatabaseConnection.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.SingularName;
            return this.GetObject<T>(key);
        }

        public T GetObject<T>(string key)
            where T : class, IObject => this.Objects.TryGetValue(key, out var @object) ? (T)@object : null;

        public object GetValue(string key) => this.Values[key];

        public T GetValue<T>(string key) => (T)this.GetValue(key);
    }
}
