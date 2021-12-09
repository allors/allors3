// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Autotest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Allors.E2E.Angular;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public partial class Reflection
    {
        public AppRoot AppRoot { get; }

        public IMetaPopulation MetaPopulation { get; }

        public Reflection(AppRoot appRoot, IMetaPopulation metaPopulation)
        {
            this.AppRoot = appRoot;
            this.MetaPopulation = metaPopulation;
        }

        public async Task<Dictionary<string, MetaExtension>> MetaExtensions()
        {
            await this.AppRoot.Page.WaitForAngular();

            var metaExtensions = new Dictionary<string, MetaExtension>();

            var meta = await this.Get("meta");
            var dialogs = await this.Get("dialogs");

            using (var reader = new JsonTextReader(new StringReader(meta)))
            {
                var jsonMetaExtensions = (JArray)await JToken.ReadFromAsync(reader);

                void Setter(MetaExtension metaExtension, JToken json)
                {
                    metaExtension.List = json["list"]?.Value<string>();
                    metaExtension.Overview = json["overview"]?.Value<string>();
                }

                MetaExtension.Load(metaExtensions, jsonMetaExtensions, Setter);
            }

            using (var reader = new JsonTextReader(new StringReader(dialogs)))
            {
                var jsonDialogs = await JToken.ReadFromAsync(reader);

                var create = jsonDialogs["create"] as JArray;
                var edit = jsonDialogs["create"] as JArray;

                void CreateSetter(MetaExtension metaExtension, JToken json) => metaExtension.Create = json["component"]?.Value<string>();

                MetaExtension.Load(metaExtensions, create, CreateSetter);

                void EditSetter(MetaExtension metaExtension, JToken json) => metaExtension.Edit = json["component"]?.Value<string>();

                MetaExtension.Load(metaExtensions, edit, EditSetter);
            }

            return metaExtensions;
        }
        public async Task<Menu> Menu()
        {
            await this.AppRoot.Page.WaitForAngular();

            var menuJson = await this.Get("menu");

            using (var reader = new JsonTextReader(new StringReader(menuJson)))
            {
                var jsonMenu = (JArray)await JToken.ReadFromAsync(reader);

                var menu = new Menu
                {
                    Reflection = this,
                };

                menu.Load(jsonMenu);

                return menu;
            }
        }

        private async Task<string> Get(string property) => await this.AppRoot.Locator.EvaluateAsync<string>($@"(element) => element.allors.reflection.{property}");
    }
}
