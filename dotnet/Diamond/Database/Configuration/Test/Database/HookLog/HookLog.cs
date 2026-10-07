// <copyright file="HookLog.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Domain;

    // The one log of the hooks of every domain of the population. Level1 and the domains that
    // extend it record through ILevel1Log, the plug-in Plugin1 through IPlugin1Log; the concrete
    // domain provides one list for both, so the tests read the order in which the hooks ran.
    public class HookLog : ILevel1Log, IPlugin1Log
    {
        private readonly List<HookLogEntry> entries = new List<HookLogEntry>();

        public IReadOnlyList<HookLogEntry> Entries => this.entries;

        public void Record(string domain, string hook, object subject = null) => this.entries.Add(new HookLogEntry(domain, hook, subject));
    }

    public sealed record HookLogEntry(string Domain, string Hook, object Subject);
}
