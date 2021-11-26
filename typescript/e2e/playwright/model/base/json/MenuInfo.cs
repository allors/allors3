// <copyright file="MenuInfo.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    public partial class MenuInfo
    {
        public string tag { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        public string icon { get; set; }
        public MenuInfo[] children { get; set; }
    }
}