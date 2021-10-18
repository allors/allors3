// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Tests;

namespace Autotest
{
    using System.Collections.Generic;
    using System.IO;
    using Allors.Workspace.Meta;
    using Angular;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public partial class Model
    {
        public M MetaPopulation { get; set; }

        public Dictionary<string, MetaExtension> MetaExtensionByTag { get; } = new Dictionary<string, MetaExtension>();

        public Project Project { get; set; }

        public Menu Menu { get; set; }

        public ValidationLog Validate() => new ValidationLog();

        public void LoadProject(FileInfo fileInfo)
        {
            using (var file = File.OpenText(fileInfo.FullName))
            using (var reader = new JsonTextReader(file))
            {
                var jsonProject = (JObject)JToken.ReadFrom(reader);
                this.Project = new Project
                {
                    Model = this,
                };

                this.Project.Load(jsonProject);
            }
        }

        public void LoadMetaExtensions(AllorsInfo allors)
        {
            foreach (var metaInfo in allors.meta)
            {
                var extension = this.GetOrCreateExtension(metaInfo.tag);
                extension.List = metaInfo.list;
                extension.Overview = metaInfo.overview;
            }
        }

        public void LoadMenu(AllorsInfo allors)
        {
            this.Menu = new Menu
            {
                Model = this,
            };

            this.Menu.Load(allors.menu);
        }

        public void LoadDialogs(AllorsInfo allors)
        {
            if (allors.dialog == null)
            {
                return;
            }

            foreach (var createInfo in allors.dialog.create)
            {
                var extension = this.GetOrCreateExtension(createInfo.tag);
                extension.Create = createInfo.component;
            }

            foreach (var editInfo in allors.dialog.edit)
            {
                var extension = this.GetOrCreateExtension(editInfo.tag);
                extension.Edit = editInfo.component;
            }
        }

        private MetaExtension GetOrCreateExtension(string tag)
        {
            if (!this.MetaExtensionByTag.TryGetValue(tag, out var metaExtension))
            {
                metaExtension = new MetaExtension
                {
                    Tag = tag,
                };

                this.MetaExtensionByTag.Add(tag, metaExtension);
            }

            return metaExtension;
        }
    }
}
