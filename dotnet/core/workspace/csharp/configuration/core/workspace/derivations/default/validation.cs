// <copyright file="IDomainChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Configuration.Derivations.Default
{
    using System.Collections.Generic;
    using Allors.Workspace.Derivations;

    public class Validation : IValidation
    {
        private readonly List<string> errors;

        public Validation() => this.errors = new List<string>();

        public bool HasErrors { get; }

        public void AddError(string error) => this.errors.Add(error);

        public IEnumerable<string> Errors => this.errors;
    }
}
