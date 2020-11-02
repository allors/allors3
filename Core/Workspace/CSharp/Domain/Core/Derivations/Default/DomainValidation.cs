// <copyright file="IDomainChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Derivations.Default
{
    using System.Collections.Generic;

    public class DomainValidation : IDomainValidation
    {
        private readonly List<string> errors;

        public DomainValidation() => this.errors = new List<string>();

        public void AddError(string error) => this.errors.Add(error);

        public IEnumerable<string> Errors => this.errors;
    }
}
