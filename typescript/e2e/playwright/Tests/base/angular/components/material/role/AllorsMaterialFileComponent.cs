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
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialFileComponent : RoleControl
    {
        public AllorsMaterialFileComponent(IComponent container, RoleType roleType) : base(container, roleType, "a-mat-file")
        {
        }

        public ILocator InputLocator => this.Locator.Locator("input[type='file']");

        public ILocator DeleteLocator => this.Locator.Locator("mat-icon").Locator("text=delete");

        public async Task UploadAsync(FileInfo file)
        {
            await this.Page.WaitForAngular();
            await this.InputLocator.SetInputFilesAsync(file.FullName);
        }

        public async Task RemoveAsync()
        {
            await this.Page.WaitForAngular();
            await this.DeleteLocator.ClickAsync();
        }
    }
}
