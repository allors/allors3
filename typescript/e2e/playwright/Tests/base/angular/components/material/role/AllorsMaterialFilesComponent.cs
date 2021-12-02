// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using System.IO;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Media = Allors.Database.Domain.Media;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialFilesComponent : RoleControl
    {
        public AllorsMaterialFilesComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-files")
        {
        }

        public AllorsMaterialMediaComponent Media(Media media) => new AllorsMaterialMediaComponent(this, media);

        public ILocator InputLocator => this.Locator.Locator("input[type='file']");

        public async Task UploadAsync(FileInfo file)
        {
            await this.Page.WaitForAngular();
            await this.InputLocator.SetInputFilesAsync(file.FullName);
        }
    }
}
