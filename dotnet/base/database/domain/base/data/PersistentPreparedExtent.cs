// <copyright file="PersistentPreparedExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Allors.Database.Data;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.SystemTextJson;

    public partial class PersistentPreparedExtent
    {
        public IExtent Extent
        {
            get
            {
                using TextReader reader = new StringReader(this.Content);
                var protocolExtent = (Allors.Protocol.Json.Data.Extent)XmlSerializer.Deserialize(reader);
                return protocolExtent.FromJson(this.Strategy.Transaction, new UnitConvert());
            }

            set
            {
                var stringBuilder = new StringBuilder();
                using TextWriter writer = new StringWriter(stringBuilder);
                XmlSerializer.Serialize(writer, value.ToJson(new UnitConvert()));
                this.Content = stringBuilder.ToString();
            }
        }

        private static XmlSerializer XmlSerializer => new XmlSerializer(typeof(Allors.Protocol.Json.Data.Extent));
    }
}
