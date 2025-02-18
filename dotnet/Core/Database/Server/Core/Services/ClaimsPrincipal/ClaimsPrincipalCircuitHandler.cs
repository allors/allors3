// <copyright file="PolicyService.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.Server.Circuits;
    using System.Threading.Tasks;
    using System.Threading;
    using System;

    internal sealed class ClaimsPrincipalCircuitHandler : CircuitHandler, IDisposable
    {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        private readonly IClaimsPrincipalService claimsPrincipalService;

        public ClaimsPrincipalCircuitHandler(AuthenticationStateProvider authenticationStateProvider, IClaimsPrincipalService claimsPrincipalService)
        {
            this.authenticationStateProvider = authenticationStateProvider;
            this.claimsPrincipalService = claimsPrincipalService;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            this.authenticationStateProvider.AuthenticationStateChanged += this.AuthenticationChanged;
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        private void AuthenticationChanged(Task<AuthenticationState> task)
        {
            _ = UpdateAuthentication(task);

            async Task UpdateAuthentication(Task<AuthenticationState> task)
            {
                try
                {
                    var state = await task;
                    this.claimsPrincipalService.User = state.User;
                }
                catch
                {
                }
            }
        }

        public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            var state = await this.authenticationStateProvider.GetAuthenticationStateAsync();
            this.claimsPrincipalService.User = state.User;
        }

        public void Dispose() => this.authenticationStateProvider.AuthenticationStateChanged -= this.AuthenticationChanged;
    }
}
