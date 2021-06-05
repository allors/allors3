namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Collections.Generic;
    using System.Data;
    using Meta;
    using Microsoft.Data.SqlClient;

    public interface ICommand
    {
        CommandType CommandType { get; set; }

        string CommandText { get; set; }

        void AddInParameter(string parameterName, object value);

        void AddObjectParameter(long objectId);

        void AddTypeParameter(IClass @class);

        void AddCountParameter(int count);

        void AddCompositeRoleParameter(long objectId);

        void AddAssociationParameter(long objectId);

        void AddObjectTableParameter(IEnumerable<Reference> references);

        void AddObjectTableParameter(IEnumerable<long> objectIds);

        void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations);

        void AddAssociationTableParameter(long objectId);

        object ExecuteScalar();

        void ExecuteNonQuery();

        DataReader ExecuteReader();

        object GetValue(DataReader reader, int tag, int i);
    }
}
