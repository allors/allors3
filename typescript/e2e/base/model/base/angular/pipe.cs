// <copyright file="Pipe.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Newtonsoft.Json.Linq;

namespace Autotest.Angular
{
    public partial class Pipe
    {
        public Project Project { get; set; }

        public JToken Json { get; set; }

        public Reference Reference { get; set; }

        public void BaseLoad()
        {
        }
    }
}
