// <copyright file="ReadSecurityKey.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Globalization;

    internal sealed class ReadSecurityKey : IEquatable<ReadSecurityKey>
    {
        public ReadSecurityKey(string kind, long id, long version)
        {
            if (kind != "grant" && kind != "revocation")
            {
                throw new ArgumentException("Use security kind 'grant' or 'revocation'.", nameof(kind));
            }

            this.Kind = kind;
            this.Id = ReadStoreValues.Id(id, nameof(id));
            this.Version = ReadStoreValues.Version(version, nameof(version));
        }

        public string Kind { get; }

        public long Id { get; }

        public long Version { get; }

        internal string CurrentKey => this.Kind + ":" + this.Id.ToString(CultureInfo.InvariantCulture);

        internal string CanonicalKey => this.CurrentKey + ":" + this.Version.ToString(CultureInfo.InvariantCulture);

        public bool Equals(ReadSecurityKey other) => other != null && this.Kind == other.Kind && this.Id == other.Id && this.Version == other.Version;

        public override bool Equals(object obj) => this.Equals(obj as ReadSecurityKey);

        public override int GetHashCode()
        {
            unchecked
            {
                return (((this.Kind.GetHashCode() * 397) ^ this.Id.GetHashCode()) * 397) ^ this.Version.GetHashCode();
            }
        }
    }
}
