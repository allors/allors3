// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using Database.Meta;

    public class DialogsInfo
    {
        public DialogInfo[] Create { get; set; }

        public DialogInfo[] Edit { get; set; }

        internal void Init(ApplicationInfo applicationInfo)
        {
            var m = applicationInfo.AppRoot.M;

            foreach (var createInfo in this.Create)
            {
                var componentInfo = applicationInfo.GetOrCreateComponentInfo(createInfo.Component);
                componentInfo.Create = (IComposite)m.FindByTag(createInfo.Tag);
            }

            foreach (var editInfo in this.Edit)
            {
                var componentInfo = applicationInfo.GetOrCreateComponentInfo(editInfo.Component);
                componentInfo.Edit = (IComposite)m.FindByTag(editInfo.Tag);
            }
        }
    }
}
