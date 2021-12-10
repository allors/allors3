// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Task = System.Threading.Tasks.Task;

    public partial class ApplicationInfo
    {
        public AppRoot AppRoot { get; }

        public IDictionary<string, ComponentInfo> ComponentInfoByName { get; }

        private ApplicationInfo(AppRoot appRoot)
        {
            this.AppRoot = appRoot;

            this.ComponentInfoByName = this.GetType().GetTypeInfo().Assembly
                .GetTypes()
                .Where(type => type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IComponent)))
                .ToDictionary(v => v.Name, v => new ComponentInfo(this) { Type = v });
        }

        public static async Task<ApplicationInfo> New(AppRoot appRoot)
        {
            var application = new ApplicationInfo(appRoot);
            await application.Init();
            return application;
        }

        private async Task Init()
        {
            var m = this.AppRoot.M;

            var dialogInfo = await this.AppRoot.GetDialogInfo();

            foreach (var createInfo in dialogInfo.Create)
            {
                if (!this.ComponentInfoByName.TryGetValue(createInfo.Component, out var componentInfo))
                {
                    componentInfo = new ComponentInfo(this);
                    this.ComponentInfoByName.Add(createInfo.Component, componentInfo);
                }

                componentInfo.Create = (IComposite)m.FindByTag(createInfo.Tag);
            }

            foreach (var editInfo in dialogInfo.Edit)
            {
                if (!this.ComponentInfoByName.TryGetValue(editInfo.Component, out var componentInfo))
                {
                    componentInfo = new ComponentInfo(this);
                    this.ComponentInfoByName.Add(editInfo.Component, componentInfo);
                }

                componentInfo.Edit = (IComposite)m.FindByTag(editInfo.Tag);
            }

            var navigationInfo = await this.AppRoot.GetNavigationInfo();

            var routeInfo = await this.AppRoot.GetRouteInfo();

            var menuInfo = await this.AppRoot.GetMenuInfo();
        }
    }
}
