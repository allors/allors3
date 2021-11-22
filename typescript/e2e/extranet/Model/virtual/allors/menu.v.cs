// <copyright file="Menu.v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Tests;

namespace Autotest
{
    using Newtonsoft.Json.Linq;

    public partial class Menu
    {
        public void Load(MenuInfo[] json)
        {
            this.BaseLoad(json);
            this.CustomLoad(json);
        }
    }
}
