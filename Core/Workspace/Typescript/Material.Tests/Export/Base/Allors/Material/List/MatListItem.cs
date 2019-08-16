// <copyright file="MatListItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Components
{
    using System.Diagnostics.CodeAnalysis;
    using OpenQA.Selenium;

    public class MatListItem
    {
        public MatListItem(IWebDriver driver, IWebElement element)
        {
            this.Driver = driver;
            this.Element = element;
        }

        public IWebDriver Driver { get; }

        public IWebElement Element { get; }

        public void Click()
        {
            this.Driver.WaitForAngular();
            this.Element.Click();
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class MatListItem<T> : MatListItem where T : Component
    {
        public MatListItem(T page, IWebElement element)
            : base(page.Driver, element)
        {
            this.Page = page;
        }

        public T Page { get; }

        public new T Click()
        {
            base.Click();
            return this.Page;
        }
    }
}