// <copyright file="ReadConfiguration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    /// <summary>
    /// Deployment-owned representation revisions, captured independently of request scope fields.
    /// Change a revision when metadata, encodings, projection or authorization policy changes.
    /// </summary>
    public sealed class ReadConfiguration
    {
        private readonly Dictionary<string, string> revisionByWorkspace;

        public ReadConfiguration(IReadOnlyDictionary<string, string> revisionByWorkspace)
        {
            ArgumentNullException.ThrowIfNull(revisionByWorkspace);
            this.revisionByWorkspace = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in revisionByWorkspace)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                {
                    throw new ArgumentException("Configure a nonempty workspace name and representation revision for every admitted workspace read.", nameof(revisionByWorkspace));
                }

                this.revisionByWorkspace.Add(entry.Key, entry.Value);
            }
        }

        internal string Revision(IMetaPopulation metaPopulation, string workspaceName)
        {
            if (string.IsNullOrWhiteSpace(workspaceName) || !metaPopulation.WorkspaceNames.Contains(workspaceName, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"Workspace '{workspaceName}' is unknown. Configure the host with a workspace defined by this database's metadata.");
            }

            if (!this.revisionByWorkspace.TryGetValue(workspaceName, out var revision))
            {
                throw new InvalidOperationException($"Workspace '{workspaceName}' has no representation revision. Configure its deployment-owned revision before admitting reads.");
            }

            return revision;
        }
    }
}
