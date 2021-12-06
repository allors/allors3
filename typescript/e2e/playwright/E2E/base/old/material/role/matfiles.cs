// <copyright file="MatFiles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using Allors.Database.Meta;
    using Microsoft.Playwright;
    using Tests;
    using Task = System.Threading.Tasks.Task;

    public class MatFiles : SelectorComponent
    {
        public MatFiles(IPage page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page, m) =>
                this.Selector = $"a-mat-files{this.ByScopesPredicate(scopes)} [@data-allors-roletype='{roleType.RelationType.Tag}']";

        public override string Selector { get; }

        public ILocator Input => this.Page.Locator($"{this.Selector} input[type='file']");

        public ILocator Delete => this.Page.Locator($"{this.Selector} mat-icon[contains(text(), 'delete')]");

        public async Task Upload(string fileName)
        {
            var file = new FileInfo(fileName);

            await this.Page.WaitForAngular();

            await this.Input.FillAsync(file.FullName);
        }

        public async Task Remove()
        {
            await this.Page.WaitForAngular();

            await this.Delete.ClickAsync();
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FilesMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class MatFiles<T> : MatFiles where T : Component
    {
        public MatFiles(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Page, m, roleType, scopes) =>
            this.Page = page;

        public new T Page { get; }

        public new async Task<T> Upload(string fileName)
        {
            await base.Upload(fileName);
            return this.Page;
        }

        public new async Task<T> Remove()
        {
            await base.Remove();
            return this.Page;
        }
    }
}
