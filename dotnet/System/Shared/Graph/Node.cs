// <copyright file="ObjectsNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Graph
{
    using System;
    using System.Collections.Generic;

    public class Node<T> : IEquatable<Node<T>>
        where T : class
    {
        private readonly T @object;

        private bool visited;
        private Node<T>? previousRoot;
        private HashSet<Node<T>>? dependencies;

        public Node(T @object)
        {
            this.@object = @object;
        }

        internal void Reset()
        {
            this.visited = false;
            this.previousRoot = null;
        }

        public void Execute(Action<T> execute) => this.Execute(this, execute);

        public void AddDependency(Node<T> node)
        {
            this.dependencies ??= new HashSet<Node<T>>();
            this.dependencies.Add(node);
        }

        public bool Equals(Node<T> other) => other != null && this.@object.Equals(other.@object);

        public override bool Equals(object obj) => this.Equals((Node<T>)obj);

        public override int GetHashCode() => this.@object.GetHashCode();

        public override string ToString() => this.@object.ToString();

        private void Execute(Node<T> currentRoot, Action<T> execute)
        {
            if (this.visited)
            {
                if (this.previousRoot != null && currentRoot.Equals(this.previousRoot))
                {
                    throw new Exception("This populate has a cycle. (" + this.previousRoot + " -> " + this + ")");
                }

                return;
            }

            this.visited = true;
            this.previousRoot = currentRoot;

            if (this.dependencies != null)
            {
                foreach (var dependency in this.dependencies)
                {
                    dependency.Execute(currentRoot, execute);
                }
            }

            execute(this.@object);

            this.previousRoot = null;
        }


    }
}
