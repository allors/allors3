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

using System.IO;
using Microsoft.Extensions.Logging;

namespace Allors.Integration.Load
{
    public class Load
    {
        public Load(Staging.Staging staging, Population population, ILoggerFactory loggerFactory, DirectoryInfo dataPath)
        {
            this.LoggerFactory = loggerFactory;

            this.Loaders = new Loader[]
            {
                new PersonLoader(staging, population, loggerFactory),
                new GeneralLedgerAccountLoader(staging, population, loggerFactory),
            };
        }
        
        public Loader[] Loaders { get; }

        public ILoggerFactory LoggerFactory { get; }

        public void Execute()
        {
            foreach (var loader in this.Loaders)
            {
                loader.OnBuild();
            }

            foreach (var loader in this.Loaders)
            {
                loader.OnUpdate();
            }
        }
    }
}
