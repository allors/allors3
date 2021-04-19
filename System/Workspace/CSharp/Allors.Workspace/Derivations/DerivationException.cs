// <copyright file="DerivationException.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain.Derivations
{
    using System;
    using Workspace.Derivations;

    public class DerivationException : Exception
    {
        public DerivationException(IValidation validation) => this.Validation = validation;

        public IValidation Validation { get; }

        public override string Message => string.Join("\n", this.Validation.Errors);
    }
}
