using Components;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public partial class Sidenav
    {
        public Button UserProfile => new Button(this.Driver, this.M, By.CssSelector(@"button[mattooltip=""Edit user profile""]"));
    }
}