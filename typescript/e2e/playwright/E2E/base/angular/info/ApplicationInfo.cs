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
    using Task = System.Threading.Tasks.Task;

    public partial class ApplicationInfo
    {
        public AppRoot AppRoot { get; }

        public IDictionary<string, ComponentInfo> ComponentInfoByName { get; }

        public IDictionary<string, ComponentInfo[]> ComponentInfoByFullPath { get; private set; }

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

        internal ComponentInfo GetOrCreateComponentInfo(string name)
        {
            if (this.ComponentInfoByName.TryGetValue(name, out var componentInfo))
            {
                return componentInfo;
            }

            componentInfo = new ComponentInfo(this);
            this.ComponentInfoByName.Add(name, componentInfo);
            return componentInfo;
        }

        private async Task Init()
        {
            var dialogInfo = await this.AppRoot.GetDialogsInfo();
            dialogInfo.Init(this);

            var routeInfos = await this.AppRoot.GetRouteInfos();
            foreach (var routeInfo in routeInfos)
            {
                routeInfo.Init(this);
            }

            this.ComponentInfoByFullPath = this.ComponentInfoByName
                .Values
                .GroupBy(v => v.RouteInfo?.FullPath)
                .Where(v => v.Key != null)
                .ToDictionary(v => v.Key, v => v.ToArray());

            var navigationInfos = await this.AppRoot.GetNavigationInfos();
            foreach (var navigationInfo in navigationInfos)
            {
                navigationInfo.Init(this);
            }

            var menuInfo = await this.AppRoot.GetMenuInfo();
        }
    }
}
