namespace Allors.Database
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;

    public class ProcedureInput : IProcedureInput
    {
        private readonly IObjectFactory objectFactory;
        private readonly Procedure procedure;

        public ProcedureInput(IObjectFactory objectFactory, Procedure procedure)
        {
            this.objectFactory = objectFactory;
            this.procedure = procedure;
        }


        public IDictionary<string, IObject[]> Collections => this.procedure.CollectionByName;

        public IDictionary<string, IObject> Objects => this.procedure.ObjectByName;

        public IDictionary<string, object> Values => this.procedure.ValueByName;

        public T[] GetCollection<T>()
        {
            var objectType = this.objectFactory.GetObjectType<T>();
            var key = objectType.PluralName;
            return this.GetCollection<T>(key);
        }

        public T[] GetCollection<T>(string key) => this.Collections.TryGetValue(key, out var collection) ? collection?.Cast<T>().ToArray() : null;

        public T GetObject<T>()
            where T : class, IObject
        {
            var objectType = this.objectFactory.GetObjectType<T>();
            var key = objectType.SingularName;
            return this.GetObject<T>(key);
        }

        public T GetObject<T>(string key)
            where T : class, IObject => this.Objects.TryGetValue(key, out var @object) ? (T)@object : null;

        public object GetValue(string key) => this.Values[key];

        public T GetValue<T>(string key) => (T)this.GetValue(key);

    }
}
