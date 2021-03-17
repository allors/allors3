// <copyright file="LocalLoadResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{

    public class LocalCallResult : ICallResult
    {
        internal LocalCallResult(LocalInvokeResult localInvokeResult)
        {
        }

        public bool HasErrors { get; }
        public string ErrorMessage { get; }
        public string[] VersionErrors { get; }
        public string[] AccessErrors { get; }
        public string[] MissingErrors { get; }
        public IDerivationError[] DerivationErrors { get; }
    }
}
