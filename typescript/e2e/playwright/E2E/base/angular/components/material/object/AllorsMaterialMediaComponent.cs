// <copyright file="MatInput.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Angular.Components
{
    using Microsoft.Playwright;
    using Tests;
    using Media = Allors.Database.Domain.Media;
    using Task = System.Threading.Tasks.Task;

    public class AllorsMaterialMediaComponent : ObjectControl<Media>
    {
        public AllorsMaterialMediaComponent(IComponent container, Media media) : base(container, media, "a-mat-media")
        {
        }

        public ILocator DeleteLocator => this.Locator.Locator("mat-icon").Locator("text=delete");

        public async Task RemoveAsync()
        {
            await this.Page.WaitForAngular();
            await this.DeleteLocator.ClickAsync();
        }
    }
}
