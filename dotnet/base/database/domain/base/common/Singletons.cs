// <copyright file="Singletons.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
   

    public partial class Singletons
    {
        public Singleton Instance => this.Transaction.GetSingleton();

        protected override void CorePrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.Locale);

        protected override void CoreSetup(Setup setup)
        {
            var singleton = this.Transaction.GetSingleton() ?? new SingletonBuilder(this.Transaction).Build();

            singleton.DefaultLocale = new Locales(this.Transaction).EnglishGreatBritain;
        }
    }
}
