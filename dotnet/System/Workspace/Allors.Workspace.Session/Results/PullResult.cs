// <copyright file="PullResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using System.Collections.Generic;
    using System.Linq;
    using ConnectionPullResult = Allors.Workspace.Connection.PullResult;

    public sealed class PullResult : Result, IPullResultInternals
    {
        private readonly ConnectionPullResult pulled;

        private IDictionary<string, IObject> objects;

        private IDictionary<string, IObject[]> collections;

        private IDictionary<string, object> values;

        internal PullResult(Session session, ConnectionPullResult pulled) : base(session, pulled)
        {
            this.Workspace = session.Workspace;
            this.pulled = pulled;
        }

        private IWorkspace Workspace { get; }

        public IDictionary<string, IObject> Objects => this.objects ??= this.pulled.Objects.ToDictionary(pair => pair.Key.ToUpperInvariant(), pair => this.Session.Instantiate<IObject>(pair.Value));

        public IDictionary<string, IObject[]> Collections => this.collections ??= this.pulled.Collections.ToDictionary(pair => pair.Key.ToUpperInvariant(), pair => pair.Value.Select(this.Session.Instantiate<IObject>).ToArray());

        public IDictionary<string, object> Values => this.values ??= this.pulled.Values.ToDictionary(pair => pair.Key.ToUpperInvariant(), pair => pair.Value);

        public T[] GetCollection<T>() where T : class, IObject
        {
            var objectType = this.Workspace.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.PluralName.ToUpperInvariant();
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) where T : class, IObject => this.Collections.TryGetValue(key.ToUpperInvariant(), out var collection) ? collection?.Cast<T>().ToArray() : null;

        public T GetObject<T>() where T : class, IObject
        {
            var objectType = this.Workspace.Configuration.ObjectFactory.GetObjectType<T>();
            var key = objectType.SingularName.ToUpperInvariant();
            return this.GetObject<T>(key);
        }

        public T GetObject<T>(string key) where T : class, IObject => this.Objects.TryGetValue(key.ToUpperInvariant(), out var @object) ? (T)@object : null;

        public object GetValue(string key) => this.Values[key.ToUpperInvariant()];

        public T GetValue<T>(string key)
        {
            var value = this.GetValue(key.ToUpperInvariant());

            return value switch
            {
                null => default,
                T typed => typed,
                _ => this.pulled.GetValue<T>(key),
            };
        }
    }
}
