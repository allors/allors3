// <copyright file="MatLocalised.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Diagnostics.CodeAnalysis;
    using Allors.Database.Meta;
    using OpenQA.Selenium;

    public class MatLocalised : SelectorComponent
    {
        public MatLocalised(IWebDriver driver, MetaPopulation m, RoleType roleType, params string[] scopes)
        : base(driver, m) =>
            this.Selector = By.XPath($".//a-mat-static{this.ByScopesPredicate(scopes)}//*[@data-allors-roletype='{roleType.RelationType.Tag}']");

        public MatLocalised(IWebDriver driver, MetaPopulation m, By selector)
            : base(driver, m) =>
            this.Selector = selector;

        public override By Selector { get; }

        // TODO:
        public string Value
        {
            get
            {
                this.Driver.WaitForAngular();
                var element = this.Driver.FindElement(this.Selector);
                return element.Text;
            }
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class MatLocalised<T> : MatInput where T : Component
    {
        public MatLocalised(T page, MetaPopulation m, RoleType roleType, params string[] scopes)
            : base(page.Driver, m, roleType, scopes) =>
            this.Page = page;

        public T Page { get; }

        public T Set(string value)
        {
            this.Value = value;
            return this.Page;
        }
    }
}
