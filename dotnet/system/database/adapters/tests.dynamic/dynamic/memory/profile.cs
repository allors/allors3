//------------------------------------------------------------------------------------------------- 
// <copyright file="Profile.cs" company="Allors bvba">
// Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// 
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// <summary>Defines the Default type.</summary>
//------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters.Memory
{
    public class Profile : Adapters.Profile
    {
        private IDatabase database;
        private IDatabase database2;

        public override void Dispose()
        {
            base.Dispose();
            this.database = null;
            this.database2 = null;
        }

        public override IDatabase GetDatabase() => this.database;

        public override IDatabase GetDatabase2() => this.database2;

        public override void Init()
        {
            this.database = this.CreateDatabase();
            this.database2 = this.CreateDatabase();
        }

        public override bool IsRollbackSupported() => true;

        public IDatabase CreateDatabase() => this.CreateMemoryDatabase();
    }
}
