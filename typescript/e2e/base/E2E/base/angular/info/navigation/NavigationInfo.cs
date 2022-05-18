// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System.Linq;
    using Database.Meta;

    public class NavigationInfo
    {
        public string Tag { get; set; }

        public string List { get; set; }

        public string Overview { get; set; }

        public void Init(ApplicationInfo applicationInfo)
        {
            var m = applicationInfo.AppRoot.M;
            var objectType = (IComposite)m.FindByTag(this.Tag);

            if (this.List != null)
            {
                if (applicationInfo.ComponentInfoByFullPath.TryGetValue(this.List, out var componentInfos))
                {
                    var componentInfo = componentInfos.OrderBy(v => v.RouteInfo.Depth).Last();
                    componentInfo.List.Add(objectType);
                }
            }

            if (this.Overview != null)
            {
                if (applicationInfo.ComponentInfoByFullPath.TryGetValue(this.Overview, out var componentInfos))
                {
                    var componentInfo = componentInfos.OrderBy(v => v.RouteInfo.Depth).Last();
                    componentInfo.Overview.Add(objectType);
                }
            }
        }
    }
}
