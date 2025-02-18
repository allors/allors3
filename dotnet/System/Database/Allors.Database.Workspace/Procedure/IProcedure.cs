// <copyright file="IProcedure.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    public interface IProcedure
    {
        void Execute(IProcedureContext context, IProcedureInput input, IProcedureOutput output);
    }
}
