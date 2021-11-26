// <copyright file="MenuItem.v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Tests;

namespace Autotest
{
    public partial class MenuItem
    {
        public void Load(MenuInfo menu)
        {
            this.BaseLoadMenu(menu);
            this.CustomLoadMenu(menu);
        }
    }
}
