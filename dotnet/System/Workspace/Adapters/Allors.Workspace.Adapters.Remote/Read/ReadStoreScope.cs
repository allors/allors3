// <copyright file="ReadStoreScope.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;

    /// <summary>
    /// An exact, immutable partition for retained read contents. This value does not authenticate a user.
    /// </summary>
    public sealed class ReadStoreScope : IEquatable<ReadStoreScope>
    {
        public ReadStoreScope(string population, long userId, string workspace, string revision, string context = null)
        {
            this.Population = ReadStoreValues.Required(population, nameof(population));
            this.UserId = ReadStoreValues.Id(userId, nameof(userId));
            this.Workspace = ReadStoreValues.Required(workspace, nameof(workspace));
            this.Revision = ReadStoreValues.Required(revision, nameof(revision));
            this.Context = context;
        }

        public string Population { get; }

        public long UserId { get; }

        public string Workspace { get; }

        public string Revision { get; }

        public string Context { get; }

        public bool Equals(ReadStoreScope other) => other != null && this.Population == other.Population &&
            this.UserId == other.UserId && this.Workspace == other.Workspace && this.Revision == other.Revision && this.Context == other.Context;

        public override bool Equals(object obj) => this.Equals(obj as ReadStoreScope);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = this.Population.GetHashCode();
                hash = (hash * 397) ^ this.UserId.GetHashCode();
                hash = (hash * 397) ^ this.Workspace.GetHashCode();
                hash = (hash * 397) ^ this.Revision.GetHashCode();
                return (hash * 397) ^ (this.Context?.GetHashCode() ?? 0);
            }
        }
    }
}
