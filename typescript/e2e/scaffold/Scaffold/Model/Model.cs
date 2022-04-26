namespace Scaffold
{
    using AngleSharp;

    public class Model
    {
        public string Cls { get; private set; }

        public string Ns { get; private set; }

        public ComponentModel[] Components { get; private set; }

        public Model(string cls, string ns)
        {
            this.Cls = cls;
            this.Ns = ns;
        }

        public async Task Init(string input)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(input));
            this.Components = document.All
                .Where(m => m.LocalName.ToLowerInvariant().StartsWith("a-", StringComparison.InvariantCulture))
                .Select(v => new ComponentModel(v))
                .ToArray();
        }
    }
}
