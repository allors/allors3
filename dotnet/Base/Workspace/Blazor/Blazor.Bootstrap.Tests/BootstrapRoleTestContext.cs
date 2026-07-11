// <copyright file="BootstrapRoleTestContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Blazor.Bootstrap.Tests
{
    using BlazorStrap;
    using Bunit;

    /// <summary>
    /// Base bUnit context that wires up the services the BlazorStrap V5 components need to render:
    /// the BlazorStrap service and a loose JS runtime (the BS components touch <c>IJSRuntime</c>).
    /// </summary>
    public abstract class BootstrapRoleTestContext : TestContext
    {
        protected BootstrapRoleTestContext()
        {
            this.Services.AddBlazorStrap();
            this.JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
