// <copyright file="IWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System;

    public abstract class Result : IResult
    {
        private IDerivationError[] derivationErrors;

        public bool HasErrors => throw new NotImplementedException();

        public string ErrorMessage => throw new NotImplementedException();

        public string[] VersionErrors => throw new NotImplementedException();

        public string[] AccessErrors => throw new NotImplementedException();

        public string[] MissingErrors => throw new NotImplementedException();

        public IDerivationError[] DerivationErrors => throw new NotImplementedException();
    }
}
