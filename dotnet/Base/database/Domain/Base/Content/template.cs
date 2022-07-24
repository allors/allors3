// <copyright file="Template.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    using Allors.Document.OpenDocument;

    public partial class Template
    {
        public object Object => this.Strategy.Transaction.Database.Services.Get<ITemplateObjectCache>().Get(this);

        public byte[] Render(object model, IDictionary<string, byte[]> images = null)
        {
            var properties = model.GetType().GetProperties();
            var dictionary = properties.ToDictionary(property => property.Name, property => property.GetValue(model));
            return this.Render(dictionary, images);
        }

        public byte[] Render(IDictionary<string, object> model, IDictionary<string, byte[]> images = null)
        {
            var subject = this.Object;

            if (subject is OpenDocumentTemplate openDocumentTemplate)
            {
                return openDocumentTemplate.Render(model, images);
            }

            return null;
        }
    }
}
