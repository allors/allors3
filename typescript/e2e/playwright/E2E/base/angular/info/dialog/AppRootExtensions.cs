// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Autotest
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.E2E.Angular;

    public static partial class AppRootExtensions
    {
        public static async Task<DialogInfo> GetDialogInfo(this AppRoot @this)
        {
            var jsonString = await @this.GetAllors("meta");
            return JsonSerializer.Deserialize<DialogInfo>(
                jsonString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
