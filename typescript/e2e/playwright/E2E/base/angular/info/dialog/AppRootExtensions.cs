// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.E2E.Angular.Info
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.E2E.Angular;

    public static partial class AppRootExtensions
    {
        public static async Task<DialogsInfo> GetDialogsInfo(this AppRoot @this)
        {
            var jsonString = await @this.GetAllors("dialog");
            return JsonSerializer.Deserialize<DialogsInfo>(
                jsonString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
