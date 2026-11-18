// <copyright file="MappingArrayParameter.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    public class MappingArrayParameter
    {
        public readonly string InvocationName;

        public MappingArrayParameter(Mapping mapping, string name)
        {
            this.InvocationName = string.Format(mapping.ParamInvocationFormat, name);
        }
    }
}
