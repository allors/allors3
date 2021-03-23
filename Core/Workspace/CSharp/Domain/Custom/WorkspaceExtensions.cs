// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Derivations;
    using Domain;

    public static partial class WorkspaceExtensions
    {
        public static void RegisterDerivations(this @IWorkspace @this)
        {
            var m = @this.Context().M;
            var derivations = new IDerivation[]
            {
                // Custom
            };

            foreach (var derivation in derivations)
            {
                @this.DomainDerivationById.Add(derivation.Id, derivation);
            }
        }
    }
}
