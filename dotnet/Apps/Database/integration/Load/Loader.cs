// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectsBase.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Allors.Database;
using Allors.Integration;

namespace Allors.Integration.Load
{
    public abstract class Loader
    {
        protected Loader(Staging.Staging staging, Population population)
        {
            this.Staging = staging;
            this.Population = population;
        }

        public Staging.Staging Staging { get; }

        public Population Population { get; }

        public ITransaction Transaction => this.Population.Transaction;

        public abstract void OnBuild();

        public abstract void OnUpdate();
    }
}
