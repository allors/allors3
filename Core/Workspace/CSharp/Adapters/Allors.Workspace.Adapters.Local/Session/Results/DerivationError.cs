// <copyright file="DerivationError.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using Protocol.Database;

    public class DerivationError : IDerivationError
    {
        public DerivationError(ResponseDerivationError responseDerivationError) => this.ResponseDerivationError = responseDerivationError;

        public ResponseDerivationError ResponseDerivationError { get; }

        public string ErrorMessage => this.ResponseDerivationError.M;

        public string[][] Roles => this.ResponseDerivationError.R;
    }
}
