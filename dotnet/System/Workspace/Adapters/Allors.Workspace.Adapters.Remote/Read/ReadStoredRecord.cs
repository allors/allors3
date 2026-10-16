// <copyright file="ReadStoredRecord.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal sealed class ReadStoredRecord
    {
        private readonly IDictionary<string, object> roles;

        public ReadStoredRecord(ReadStoreScope scope, long id, string classTag, long dataVersion,
            IEnumerable<ReadSecurityKey> grants, IEnumerable<ReadSecurityKey> revocations, IReadOnlyDictionary<string, object> roles)
        {
            this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.Id = ReadStoreValues.Id(id, nameof(id));
            this.ClassTag = ReadStoreValues.Required(classTag, nameof(classTag));
            this.DataVersion = ReadStoreValues.Version(dataVersion, nameof(dataVersion));
            this.Grants = Array.AsReadOnly(ReadStoreValues.Security(grants, "grant"));
            this.Revocations = Array.AsReadOnly(ReadStoreValues.Security(revocations, "revocation"));
            this.roles = (IDictionary<string, object>)ReadStoreValues.Copy(roles ?? throw new ArgumentNullException(nameof(roles)));
            this.Identity = this.Id.ToString(CultureInfo.InvariantCulture) + "|" + this.ClassTag.Length.ToString(CultureInfo.InvariantCulture) + ":" + this.ClassTag + "|" +
                this.DataVersion.ToString(CultureInfo.InvariantCulture) + "|" + string.Join(",", this.Grants.Select(v => v.CanonicalKey)) + "|" +
                string.Join(",", this.Revocations.Select(v => v.CanonicalKey));
        }

        public ReadStoreScope Scope { get; }

        public long Id { get; }

        public string ClassTag { get; }

        public long DataVersion { get; }

        public IReadOnlyList<ReadSecurityKey> Grants { get; }

        public IReadOnlyList<ReadSecurityKey> Revocations { get; }

        /// <summary>
        /// Each access returns a detached value tree. Editing that tree cannot alter the retained payload.
        /// </summary>
        public IReadOnlyDictionary<string, object> Roles => (IReadOnlyDictionary<string, object>)ReadStoreValues.Copy(this.roles);

        internal string Identity { get; }

        internal bool SameBody(ReadStoredRecord other) => ReadStoreValues.Equal(this.roles, other.roles);
    }
}
