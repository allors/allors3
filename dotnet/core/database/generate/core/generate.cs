namespace Allors.Meta.Generation
{
    using System.IO;
    using Database.Meta;

    public static class Generate
    {
        public static Log Execute(MetaPopulation metaPopulation, string template, string output, string workspaceName = null)
        {
            var log = new GenerateLog();

            var templateFileInfo = new FileInfo(template);
            var stringTemplate = new StringTemplate(templateFileInfo);
            var outputDirectoryInfo = new DirectoryInfo(output);

            stringTemplate.Generate(metaPopulation, workspaceName, outputDirectoryInfo, log);

            return log;
        }
    }
}
