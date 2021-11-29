// <copyright file="Input.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Components
{
    using System.Diagnostics.CodeAnalysis;
    using Allors.Database.Meta;
    using OpenQA.Selenium;
    using SeleniumExtras.PageObjects;

    public class Input : SelectorComponent
    {
        public Input(IWebDriver driver, MetaPopulation m, params By[] selectors)
            : base(driver, m) =>
            this.Selector = selectors.Length == 1 ? selectors[0] : new ByChained(selectors);

        public Input(IWebDriver driver, MetaPopulation m, string kind, string value, params string[] scopes)
            : base(driver, m)
        {
            switch (kind.ToLowerInvariant())
            {
                case "id":
                    this.Selector = By.XPath($".//input[@id='{value}'{this.ByScopesAnd(scopes)}]");
                    break;

                case "name":
                    this.Selector = By.XPath($".//input[@name='{value}'{this.ByScopesAnd(scopes)}]");
                    break;

                case "formcontrolname":
                    this.Selector = By.XPath($".//input[@formcontrolname='{value}'{this.ByScopesAnd(scopes)}]");
                    break;

                default:
                    this.Selector = By.XPath($".//input{this.ByScopesPredicate(scopes)}");
                    break;
            }
        }

        public override By Selector { get; }

        public string Value
        {
            get
            {
                this.Driver.WaitForAngular();
                var element = this.Driver.FindElement(this.Selector);
                return element.GetAttribute("value");
            }

            set
            {
                this.Driver.WaitForAngular();
                var element = this.Driver.FindElement(this.Selector);
                this.ScrollToElement(element);
                element.Clear();
                element.SendKeys(value);
                element.SendKeys(Keys.Tab);
            }
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class Input<T> : Input where T : Component
    {
        public Input(T page, MetaPopulation m, params By[] selectors)
            : base(page.Driver, m, selectors) =>
            this.Page = page;

        public Input(T page, MetaPopulation m, string kind, string value, params string[] scopes)
            : base(page.Driver, m, kind, value, scopes) =>
            this.Page = page;

        public T Page { get; }

        public T Set(string value)
        {
            this.Value = value;
            return this.Page;
        }
    }
}
