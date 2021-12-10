// <copyright file="Component.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Database.Meta;
    using Task = System.Threading.Tasks.Task;

    public partial class ApplicationInfo
    {
        public AppRoot AppRoot { get; }

        public MetaPopulation M { get; private set; }

        public IDictionary<string, Type> ComponentTypeByName { get; private set; }

        public IDictionary<IComposite, string> CreateComponentByObjectType { get; private set; }

        public IDictionary<IComposite, string> EditComponentByObjectType { get; private set; }

        private ApplicationInfo(AppRoot appRoot)
        {
            this.AppRoot = appRoot;
            this.M = this.AppRoot.M;

            this.ComponentTypeByName = this.GetType().GetTypeInfo().Assembly
                .GetTypes()
                .Where(type => type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IComponent)))
                .ToDictionary(v => v.Name);
        }

        public static async Task<ApplicationInfo> New(AppRoot appRoot)
        {
            var application = new ApplicationInfo(appRoot);
            await application.Init();
            return application;
        }

        private async Task Init()
        {
            var dialogInfo = await this.AppRoot.GetDialogInfo();

            this.CreateComponentByObjectType = dialogInfo.Create.ToDictionary(v => this.M.FindByTag(v.Tag) as IComposite, v => v.Component);
            this.EditComponentByObjectType = dialogInfo.Edit.ToDictionary(v => this.M.FindByTag(v.Tag) as IComposite, v => v.Component);

            var menuInfo = await this.AppRoot.GetMenuInfo();

            var navigationInfo = await this.AppRoot.GetNavigationInfo();

            var routeInfo = await this.AppRoot.GetRouteInfo();
        }
    }
}
