// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class FaceToFaceCommunicationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdFaceToFaceCommunications = changeSet.Created.Select(v=>v.GetObject()).OfType<FaceToFaceCommunication>();

                foreach(var faceToFaceCommunication in createdFaceToFaceCommunications)
                {
                    faceToFaceCommunication.WorkItemDescription = $"Meeting with {faceToFaceCommunication.ToParty.PartyName} about {faceToFaceCommunication.Subject}";
                }
            }
        }

        public static void FaceToFaceCommunicationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("ebbca7ad-cc6f-4f9f-b6bb-2d1ffb0c614c")] = new FaceToFaceCommunicationCreationDerivation();
        }
    }
}
