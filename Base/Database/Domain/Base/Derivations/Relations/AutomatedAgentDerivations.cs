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
        public class AutomatedAgentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdAutomatedAgent = changeSet.Created.Select(session.Instantiate).OfType<AutomatedAgent>();

                foreach(var automatedAgent in createdAutomatedAgent)
                {
                    automatedAgent.PartyName = automatedAgent.Name;
                }
            }
        }

        public static void AutomatedAgentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d43a9278-d671-4b4a-9c9a-6aebc3e9edd2")] = new AutomatedAgentCreationDerivation();
        }
    }
}
