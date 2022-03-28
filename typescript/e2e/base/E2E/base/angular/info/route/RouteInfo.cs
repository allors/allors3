// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    public class RouteInfo
    {
        public string Path { get; set; }

        public string PathMatch { get; set; }

        public string Component { get; set; }

        public string RedirectTo { get; set; }

        public RouteInfo[] Children { get; set; }

        public RouteInfo Parent { get; internal set; }

        public string FullPath
        {
            get
            {
                var parentPath = this.Parent?.FullPath ?? "/";

                if (string.IsNullOrEmpty(this.Path))
                {
                    return parentPath;
                }

                return parentPath + (!parentPath.EndsWith("/") ? "/" : string.Empty) + this.Path;
            }
        }

        public int Depth => (this.Parent?.Depth ?? 0) + 1;

        internal void ConnectParentToChildren()
        {
            if (this.Children == null)
            {
                return;
            }

            foreach (var child in this.Children)
            {
                child.Parent = this;
                child.ConnectParentToChildren();
            }
        }

        internal void Init(ApplicationInfo applicationInfo)
        {
            if (this.Component != null)
            {
                var componentInfo = applicationInfo.GetOrCreateComponentInfo(this.Component);
                componentInfo.RouteInfo = this;
            }

            if (this.Children == null)
            {
                return;
            }

            foreach (var child in this.Children)
            {
                child.Init(applicationInfo);
            }
        }
    }
}
