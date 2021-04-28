
export class PullResult extends Result implements IPullResult
    {
      constructor(){
        super(session, response);

        this.Workspace = session.Workspace;

        this.Objects = response.Objects.ToDictionary(
            pair => pair.Key,
            pair => session.Get<IObject>(pair.Value),
            StringComparer.OrdinalIgnoreCase);
        this.Collections = response.Collections.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Select(session.Get<IObject>).ToArray(),
            StringComparer.OrdinalIgnoreCase);
        this.Values = response.Values.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.OrdinalIgnoreCase);
      }

        Objects: Map<string, IObject>;

        Collections: Map<string, IObject[]>;

        Values: Map<string, UnitTypes>;

        Workspace: IWorkspace;

        public T[] GetCollection<T>()
        {
            var objectType = this.Workspace.ObjectFactory.GetObjectType<T>();
            var key = objectType.PluralName;
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) => this.Collections.TryGetValue(key, out var collection) ? collection?.Cast<T>().ToArray() : null;

        public T GetObject<T>()
            where T : class, IObject
        {
            var objectType = this.Workspace.ObjectFactory.GetObjectType<T>();
            var key = objectType.SingularName;
            return this.GetObject<T>(key);
        }

        public T GetObject<T>(string key)
            where T : class, IObject => this.Objects.TryGetValue(key, out var @object) ? (T)@object : null;

        public object GetValue(string key) => this.Values[key];

        public T GetValue<T>(string key) => (T)this.GetValue(key);
    }
}
