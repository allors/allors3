namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Meta;

    public interface ICommand : IDisposable
    {
        CommandType CommandType { get; set; }

        string CommandText { get; set; }

        ISqlParameterCollection Parameters { get; }

        void AddInParameter(string parameterName, object value);

        void AddObjectParameter(long objectId);

        void AddTypeParameter(IClass @class);

        void AddCountParameter(int count);

        void AddUnitRoleParameter(IRoleType roleType, object unit);

        void AddUnitTableParameter(IRoleType roleType, IEnumerable<UnitRelation> relations);

        void AddCompositeRoleParameter(long objectId);

        void AddAssociationParameter(long objectId);

        void AddObjectTableParameter(IEnumerable<Reference> references);

        void AddObjectTableParameter(IEnumerable<long> objectIds);
        
        void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations);

        void AddAssociationTableParameter(long objectId);

        object ExecuteScalar();

        void ExecuteNonQuery();

        DataReader ExecuteReader();

        ISqlParameter CreateParameter();

        object GetValue(DataReader reader, int tag, int i);
    }
}
