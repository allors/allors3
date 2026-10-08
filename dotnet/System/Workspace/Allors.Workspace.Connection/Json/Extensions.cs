// <copyright file="Extensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection.Json
{
    using Allors.Protocol.Json;
    using Procedure = Allors.Protocol.Json.Data.Procedure;
    using Pull = Allors.Protocol.Json.Data.Pull;

    internal static class Extensions
    {
        internal static Pull ToJson(this Data.Pull pull, IUnitConvert unitConvert)
        {
            var toJsonVisitor = new ToJsonVisitor(unitConvert);
            pull.Accept(toJsonVisitor);
            return toJsonVisitor.Pull;
        }

        internal static Procedure ToJson(this Data.Procedure procedure, IUnitConvert unitConvert)
        {
            var toJsonVisitor = new ToJsonVisitor(unitConvert);
            procedure.Accept(toJsonVisitor);
            return toJsonVisitor.Procedure;
        }
    }
}
