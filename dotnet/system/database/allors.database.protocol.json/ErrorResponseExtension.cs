// <copyright file="ErrorResponseExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;
    using Derivations;

    public static class ResponseExtensions
    {
        public static void AddDerivationErrors(this Response @this, IValidation validation)
        {
            foreach (var derivationError in validation.Errors)
            {
                var derivationErrorResponse = new ResponseDerivationError
                {
                    ErrorMessage = derivationError.Message,
                    Roles = derivationError.Relations.Select(x => new[] { x.Association.Id, x.RelationType.Tag }).ToArray(),
                };

                @this.DerivationErrors = @this.DerivationErrors != null ?
                                             new List<ResponseDerivationError>(@this.DerivationErrors) { derivationErrorResponse }.ToArray() :
                                             new List<ResponseDerivationError> { derivationErrorResponse }.ToArray();
            }
        }

        public static void AddVersionError(this Response @this, IObject obj) =>
            @this.VersionErrors = @this.VersionErrors != null ?
                new List<long>(@this.VersionErrors) { obj.Id }.ToArray() :
                new List<long> { obj.Id }.ToArray();

        public static void AddAccessError(this Response @this, IObject obj) =>
            @this.AccessErrors = @this.AccessErrors != null ?
                new List<long>(@this.AccessErrors) { obj.Id }.ToArray() :
                new List<long> { obj.Id }.ToArray();

        public static void AddMissingError(this Response @this, long id) =>
            @this.MissingErrors = @this.MissingErrors != null ?
                new List<long>(@this.MissingErrors) { id }.ToArray() :
                new List<long> { id }.ToArray();
    }
}
