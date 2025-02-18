// <copyright file="IProcedureContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using Derivations;
    using Security;

    public interface IProcedureContext
    {
        ITransaction Transaction { get; }

        IAccessControl AccessControl { get; }

        void AddError(IValidation validation);
    }
}
