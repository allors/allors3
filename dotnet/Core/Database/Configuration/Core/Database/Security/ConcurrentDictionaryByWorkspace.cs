// <copyright file="IBarcodeGenerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;

    public class ConcurrentDictionaryByWorkspace<T>
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, T>> store;

        public ConcurrentDictionaryByWorkspace() => this.store = new ConcurrentDictionary<string, ConcurrentDictionary<long, T>>();

        public ConcurrentDictionary<long, T> this[string workspaceName]
        {
            get
            {
                if (this.store.TryGetValue(workspaceName, out var concurrentDictionary))
                {
                    return concurrentDictionary;
                }

                concurrentDictionary = new ConcurrentDictionary<long, T>();
                this.store[workspaceName] = concurrentDictionary;
                return concurrentDictionary;
            }
        }
    }
}
