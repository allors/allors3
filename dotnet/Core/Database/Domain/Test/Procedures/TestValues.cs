// <copyright file="TestValues.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public class TestValues : IProcedure
    {
        public void Execute(IProcedureContext context, IProcedureInput input, IProcedureOutput output)
        {
            output.AddValue("allorsBinary", new byte[] { 1, 2, 3 });
            output.AddValue("allorsBoolean", true);
            output.AddValue("allorsDateTime", new DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc));
            output.AddValue("allorsDecimal", 12.34m);
            output.AddValue("allorsDouble", 123d);
            output.AddValue("allorsInteger", 1000);
            output.AddValue("allorsString", "a string");
            output.AddValue("allorsUnique", new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF"));
        }
    }
}
