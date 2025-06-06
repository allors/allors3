// <copyright file="StringTemplate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the Default type.
// </summary>

namespace Allors.Meta.Generation
{
    using System;
    using System.IO;
    using System.Xml;
    using Antlr4.StringTemplate;
    using Antlr4.StringTemplate.Misc;
    using Model;
    using Storage;

    public class StringTemplate
    {
        private const string TemplateId = "TemplateId";
        private const string TemplateName = "TemplateName";
        private const string TemplateVersion = "TemplateVersion";
        private const string TemplateConfiguration = "TemplateConfiguration";

        private const string TemplateKey = "template";
        private const string MetaKey = "meta";
        private const string InputKey = "input";
        private const string OutputKey = "output";
        private const string GenerationKey = "generation";
        private const string DomainKey = "domain";
        private const string ObjectTypeKey = "objectType";
        private const string RelationTypeKey = "relationType";
        private const string MethodTypeKey = "methodType";
        private const string WorkspaceNameKey = "workspaceName";

        private readonly FileInfo fileInfo;

        internal StringTemplate(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;

            this.fileInfo.Refresh();
            if (!this.fileInfo.Exists)
            {
                var fullName = fileInfo.FullName;
                throw new Exception("Template file not found: " + fullName);
            }

            TemplateGroup templateGroup = new TemplateGroupFile(this.fileInfo.FullName, '$', '$');

            this.Id = Render(templateGroup, TemplateId) != null ? new Guid(Render(templateGroup, TemplateId)) : Guid.Empty;
            this.Name = Render(templateGroup, TemplateName);
            this.Version = Render(templateGroup, TemplateVersion);

            if (this.Id == Guid.Empty)
            {
                throw new Exception("Template has no id");
            }
        }

        public Guid Id { get; }

        public string Name { get; }

        public string Version { get; }

        public override string ToString() => this.Name;

        internal void Generate(MetaModel meta, string workspaceName, DirectoryInfo outputDirectory, Log log)
        {
            var validation = meta.MetaPopulation.Validate();
            if (validation.ContainsErrors)
            {
                log.Error(this, "Meta population has validation errors:");

                foreach (var error in validation.Errors)
                {
                    log.Error(this, error.ToString());
                }
                return;
            }

            try
            {
                TemplateGroup templateGroup = new TemplateGroupFile(this.fileInfo.FullName, '$', '$')
                {
                    ErrorManager = new ErrorManager(new LogAdapter(log))
                };

                var configurationTemplate = templateGroup.GetInstanceOf(TemplateConfiguration);
                configurationTemplate.Add(MetaKey, meta);
                if (!string.IsNullOrWhiteSpace(workspaceName))
                {
                    configurationTemplate.Add(WorkspaceNameKey, workspaceName);
                }

                var configurationXml = new XmlDocument();
                configurationXml.LoadXml(configurationTemplate.Render());

                var location = new Location(outputDirectory);
                foreach (XmlElement generation in configurationXml.DocumentElement.SelectNodes(GenerationKey))
                {
                    var templateName = generation.GetAttribute(TemplateKey);
                    var template = templateGroup.GetInstanceOf(templateName);
                    var output = generation.GetAttribute(OutputKey);

                    template.Add(MetaKey, meta);
                    if (!string.IsNullOrWhiteSpace(workspaceName))
                    {
                        template.Add(WorkspaceNameKey, workspaceName);
                    }

                    if (generation.HasAttribute(InputKey))
                    {
                        var input = new Guid(generation.GetAttribute(InputKey));
                        var @object = meta.Map(meta.MetaPopulation.FindById(input));

                        switch (@object)
                        {
                            case DomainModel domain:
                                template.Add(DomainKey, domain);
                                break;
                            case ObjectTypeModel objectType:
                                template.Add(ObjectTypeKey, objectType);
                                break;
                            case RelationTypeModel relationType:
                                template.Add(RelationTypeKey, relationType);
                                break;
                            case MethodTypeModel methodType:
                                template.Add(MethodTypeKey, methodType);
                                break;
                            default:
                                throw new ArgumentException(input + " was not found");
                        }
                    }

                    var result = template.Render();
                    location.Save(output, result);
                }
            }
            catch (Exception e)
            {
                log.Error(this, "Generation error : " + e.Message + "\n" + e.StackTrace);
            }
        }

        private static string Render(TemplateGroup templateGroup, string templateName)
        {
            var template = templateGroup.GetInstanceOf(templateName);
            if (template != null)
            {
                return template.Render();
            }

            return null;
        }

        private class LogAdapter : ITemplateErrorListener
        {
            private readonly Log log;

            public LogAdapter(Log log) => this.log = log;

            public void CompiletimeError(TemplateMessage msg) => this.log.Error(msg, msg.ToString());

            public void RuntimeError(TemplateMessage msg) => this.log.Error(msg, msg.ToString());

            public void IOError(TemplateMessage msg) => this.log.Error(msg, msg.ToString());

            public void InternalError(TemplateMessage msg) => this.log.Error(msg, msg.ToString());
        }
    }
}
