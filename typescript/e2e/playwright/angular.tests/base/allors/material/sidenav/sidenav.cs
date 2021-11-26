// <copyright file="Sidenav.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors.Database.Meta;
    using Components;
    using OpenQA.Selenium;
    using SeleniumExtras.PageObjects;

    public partial class Sidenav : SelectorComponent
    {
        public Sidenav(IWebDriver driver, MetaPopulation m)
        : base(driver, m) =>
            this.Selector = By.CssSelector("mat-sidenav");

        public override By Selector { get; }

        public Button Toggle => new Button(this.Driver, this.M, By.CssSelector(@"button[aria-label=""Toggle sidenav""]"));

        private void Navigate(Anchor link)
        {
            this.Driver.WaitForAngular();

            if (!link.IsVisible)
            {
                this.Toggle.Click();
                this.Driver.WaitForCondition(driver => link.IsVisible);
            }

            link.Click();
        }

        private void Navigate(Element group, Anchor link)
        {
            this.Driver.WaitForAngular();

            if (!link.IsVisible)
            {
                if (!group.IsVisible)
                {
                    this.Toggle.Click();
                    this.Driver.WaitForAngular();
                    this.Driver.WaitForCondition(driver => @group.IsVisible);
                }

                if (!link.IsVisible)
                {
                    group.Click();
                }
            }

            link.Click();
            this.Driver.WaitForAngular();
        }

        private Element Group(string name) => new Element(this.Driver, this.M, new ByChained(this.Selector, By.XPath($".//span[contains(text(), '{name}')]")));

        private Anchor Link(string href) => new Anchor(this.Driver, this.M, this.ByHref(href));

        private By ByHref(string href) => new ByChained(this.Selector, By.CssSelector($"a[href='{href}']"));
    }
}
