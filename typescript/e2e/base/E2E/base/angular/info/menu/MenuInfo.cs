// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System.Linq;

    public class MenuInfo
    {
        public string Tag { get; set; }

        public string Link { get; set; }

        public string Title { get; set; }

        public string Icon { get; set; }

        public MenuInfo[] Children { get; set; }

        public MenuInfo Parent { get; set; }

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
            if (this.Tag != null)
            {
                var componentInfo = applicationInfo.ComponentInfoByName.Values.FirstOrDefault(v => v.List?.Tag.Equals(this.Tag) == true);
                if (componentInfo != null)
                {
                    componentInfo.MenuInfo = this;
                }
            }
            else if (this.Link != null)
            {
                if (applicationInfo.ComponentInfoByFullPath.TryGetValue(this.Link, out var componentInfos))
                {
                    var componentInfo = componentInfos.OrderBy(v => v.RouteInfo.Depth).Last();
                    componentInfo.MenuInfo = this;
                }
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
