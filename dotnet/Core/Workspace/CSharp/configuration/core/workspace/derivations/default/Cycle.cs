// <copyright file="Cycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Configuration.Derivations.Default
{
    using Allors.Workspace.Derivations;

    public class Cycle : ICycle
    {
        public ISession Session { get; internal set; }

        public IChangeSet ChangeSet { get; internal set; }

        public IValidation Validation { get; internal set; }
    }
}
