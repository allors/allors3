// <copyright file="DriverManager.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Threading;
using OpenQA.Selenium;

namespace Tests
{
    public static class DriverExtensions
    {
        public static string GetGlobal(this IWebDriver @this, string name)
        {
            var script = $"return window.{name};";

            var javascriptExecutor = (IJavaScriptExecutor)@this;
            var timeOut = DateTime.Now.AddMinutes(1);
            var factor = 1;

            string result = null;
            while (result == null && timeOut > DateTime.Now)
            {
                result = (string)javascriptExecutor.ExecuteScript(script);
                Thread.Sleep(Math.Min(10 * factor++, 100));
            }

            return result;
        }
    }
}
