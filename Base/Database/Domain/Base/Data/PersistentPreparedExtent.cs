// <copyright file="PersistentPreparedExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Allors.Data;
    using Allors.Database.Protocol.Json;

    public partial class PersistentPreparedExtent
    {
        public IExtent Extent
        {
            get
            {
                using TextReader reader = new StringReader(this.Content);
                var protocolExtent = (Protocol.Json.Data.Extent)XmlSerializer.Deserialize(reader);
                return protocolExtent.FromJson(this.Strategy.Session);
            }

            set
            {
                var stringBuilder = new StringBuilder();
                using TextWriter writer = new StringWriter(stringBuilder);
                XmlSerializer.Serialize(writer, value.ToJson());
                this.Content = stringBuilder.ToString();
            }
        }

        private static XmlSerializer XmlSerializer => new XmlSerializer(typeof(Extent));
    }
}
