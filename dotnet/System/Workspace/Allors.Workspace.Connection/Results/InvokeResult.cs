// <copyright file="InvokeResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Allors.Protocol.Json.Api.Invoke;
    using Meta;

    /// <summary>
    /// What an invoke answered: the errors.
    /// </summary>
    public sealed class InvokeResult : Result
    {
        internal InvokeResult(IMetaPopulation metaPopulation, InvokeResponse response) : base(metaPopulation, response)
        {
        }
    }
}
