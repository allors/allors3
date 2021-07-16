// <copyright file="Workspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;

    public class PullResult : IPullResult
    {
        public bool HasErrors { get; }
        public string ErrorMessage { get; }
        public IEnumerable<IObject> VersionErrors { get; }
        public IEnumerable<IObject> AccessErrors { get; }
        public IEnumerable<IObject> MissingErrors { get; }
        public IEnumerable<IDerivationError> DerivationErrors { get; }
        public IDictionary<string, IObject[]> Collections { get; }
        public IDictionary<string, IObject> Objects { get; }
        public IDictionary<string, object> Values { get; }

        public T[] GetCollection<T>() where T : class, IObject => throw new System.NotImplementedException();

        public T[] GetCollection<T>(string key) where T : class, IObject => throw new System.NotImplementedException();

        public T GetObject<T>() where T : class, IObject => throw new System.NotImplementedException();

        public T GetObject<T>(string key) where T : class, IObject => throw new System.NotImplementedException();

        public object GetValue(string key) => throw new System.NotImplementedException();

        public T GetValue<T>(string key) => throw new System.NotImplementedException();
    }
}
