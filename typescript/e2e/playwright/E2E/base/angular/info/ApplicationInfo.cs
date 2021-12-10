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
            ComponentInfo ComponentInfo(string name)
            {
                if (this.ComponentInfoByName.TryGetValue(name, out var componentInfo))
                {
                    return componentInfo;
                }

                componentInfo = new ComponentInfo(this);
                this.ComponentInfoByName.Add(name, componentInfo);
                return componentInfo;
            }

            var m = this.AppRoot.M;

            var dialogInfo = await this.AppRoot.GetDialogInfo();

            foreach (var createInfo in dialogInfo.Create)
            {
                var componentInfo = ComponentInfo(createInfo.Component);
                componentInfo.Create = (IComposite)m.FindByTag(createInfo.Tag);
            }

            foreach (var editInfo in dialogInfo.Edit)
            {
                var componentInfo = ComponentInfo(editInfo.Component);
                componentInfo.Edit = (IComposite)m.FindByTag(editInfo.Tag);
            }

            var routesInfo = await this.AppRoot.GetRoutesInfo();
            var componentRoutesInfo = routesInfo
                .Flatten()
                .Where(v => !string.IsNullOrEmpty(v.Component));

            foreach (var routeInfo in componentRoutesInfo)
            {
                var componentInfo = ComponentInfo(routeInfo.Component);
                componentInfo.RouteInfo = routeInfo;
            }

            var navigationInfo = await this.AppRoot.GetNavigationInfo();

            var menuInfo = await this.AppRoot.GetMenuInfo();
        }
    }
}
