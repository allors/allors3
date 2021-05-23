// <copyright file="TreeCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using Data;
    using Domain;
    using Meta;

    public class TreeCache : ITreeCache
    {
        private readonly ConcurrentDictionary<IComposite, Node[]> trees;

        public TreeCache() => this.trees = new ConcurrentDictionary<IComposite, Node[]>();

        public Node[] Get(IComposite type) => this.trees.TryGetValue(type, out var tree) ? tree : null;

        public void Set(IComposite type, Node[] tree) => this.trees[type] = tree;
    }
}
