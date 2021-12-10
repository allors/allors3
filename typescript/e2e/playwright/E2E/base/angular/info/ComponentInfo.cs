// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System;
    using Database.Meta;

    public partial class ComponentInfo
    {
        public ApplicationInfo ApplicationInfo { get; }

        public Type Type { get; set; }

        public RouteInfo RouteInfo { get; set; }

        public IComposite Create { get; set; }

        public IComposite Edit { get; set; }

        internal ComponentInfo(ApplicationInfo applicationInfo) => this.ApplicationInfo = applicationInfo;
    }
}
