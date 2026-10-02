// <copyright file="DisableIdentityPagesConvention.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Filters;

    // Disables selected Identity area pages (returns 404) without needing an override page per path.
    // The default set closes pages that are dangerous or unsupported against AllorsUserStore: open
    // registration (anonymous user creation), the personal-data pages (DeleteAsync cascade-deletes the
    // user), and two-factor management (no authenticator-key store until a later phase). The set is
    // configurable, so an inheritor can re-enable a page (e.g. open registration) via configuration.
    public class DisableIdentityPagesConvention : IPageApplicationModelConvention
    {
        private const string IdentityAreaName = "Identity";

        private readonly HashSet<string> disabledViewEnginePaths;

        public DisableIdentityPagesConvention(IEnumerable<string> disabledViewEnginePaths) =>
            this.disabledViewEnginePaths = new HashSet<string>(disabledViewEnginePaths, StringComparer.OrdinalIgnoreCase);

        public void Apply(PageApplicationModel model)
        {
            if (string.Equals(model.AreaName, IdentityAreaName, StringComparison.OrdinalIgnoreCase) &&
                this.disabledViewEnginePaths.Contains(model.ViewEnginePath))
            {
                model.Filters.Add(new NotFoundPageFilter());
            }
        }

        private sealed class NotFoundPageFilter : IPageFilter
        {
            public void OnPageHandlerSelected(PageHandlerSelectedContext context)
            {
            }

            public void OnPageHandlerExecuting(PageHandlerExecutingContext context) => context.Result = new NotFoundResult();

            public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
            {
            }
        }
    }
}
