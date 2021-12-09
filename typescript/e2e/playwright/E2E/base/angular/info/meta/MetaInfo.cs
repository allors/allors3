// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Autotest
{
    public class MenuInfo
    {
        public string Tag { get; set; }

        public string Link { get; set; }

        public string Title { get; set; }

        public string Icon { get; set; }

        public MenuInfo[] Children { get; set; }
    }
}
