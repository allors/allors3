// <copyright file="ObjectsGraph.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Graph<T> : IEnumerable<T>
        where T : class
    {
        private readonly Dictionary<T, Node<T>> nodeByObject;

        public Graph() => this.nodeByObject = new Dictionary<T, Node<T>>();

        public Graph(IEnumerable<T> objects, Func<T, IEnumerable<T>> dependencyFunc)
        : this()
        {
            foreach (var @object in objects)
            {
                this.Add(@object);
            }

            foreach (var dependent in this.nodeByObject.Keys.ToArray())
            {
                var dependencies = dependencyFunc(dependent);
                foreach (var dependency in dependencies)
                {
                    this.AddDependency(dependent, dependency);
                }
            }
        }

        public void Invoke(Action<T> action)
        {
            foreach (var node in this.nodeByObject.Values)
            {
                node.Reset();
            }

            foreach (var node in this.nodeByObject.Values)
            {
                node.Execute(action);
            }
        }

        public Node<T> Add(T objects)
        {
            if (this.nodeByObject.TryGetValue(objects, out var node))
            {
                return node;
            }

            node = new Node<T>(objects);
            this.nodeByObject.Add(objects, node);
            return node;
        }

        public void AddDependency(T dependent, T dependency)
        {
            var dependentNode = this.Add(dependent);
            var dependencyNode = this.Add(dependency);
            dependentNode.AddDependency(dependencyNode);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var list = new List<T>();
            this.Invoke(v => list.Add(v));
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
