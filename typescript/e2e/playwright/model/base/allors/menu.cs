// <copyright file="Menu.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Tests;

namespace Autotest
{
    using System.Linq;

    public partial class Menu
    {
        public Model Model { get; set; }

        public MenuItem[] MenuItems { get; set; }

        public void BaseLoad(MenuInfo[] items) =>
            this.MenuItems = items?
                .Select(v =>
                {
                    var child = new MenuItem
                    {
                        Menu = this,
                    };

                    child.Load(v);
                    return child;
                }).ToArray();
    }
}
