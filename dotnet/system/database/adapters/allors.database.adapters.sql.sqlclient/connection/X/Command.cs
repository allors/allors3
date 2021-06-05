// <copyright file="Command.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;

    using Meta;

    public class Command : IDisposable, ICommand
    {
        internal Command(Mapping mapping, SqlCommand command)
        {
            this.Mapping = mapping;
            this.SqlCommand = command;
        }

        internal SqlParameterCollection Parameters => this.SqlCommand.Parameters;

        public CommandType CommandType
        {
            get => this.SqlCommand.CommandType;

            set => this.SqlCommand.CommandType = value;
        }

        public string CommandText
        {
            get => this.SqlCommand.CommandText;

            set => this.SqlCommand.CommandText = value;
        }

        protected Mapping Mapping { get; }

        protected SqlCommand SqlCommand { get; }

        public void Dispose() => this.SqlCommand.Dispose();

        internal SqlParameter CreateParameter() => this.SqlCommand.CreateParameter();

        public void AddInParameter(string parameterName, object value)
        {
            var sqlParameter = this.SqlCommand.Parameters.Contains(parameterName) ? this.SqlCommand.Parameters[parameterName] : null;
            if (sqlParameter == null)
            {
                sqlParameter = this.SqlCommand.CreateParameter();
                sqlParameter.ParameterName = parameterName;
                if (value is DateTime)
                {
                    sqlParameter.SqlDbType = SqlDbType.DateTime2;
                }

                this.SqlCommand.Parameters.Add(sqlParameter);
            }

            if (value == null || value == DBNull.Value)
            {
                this.SqlCommand.Parameters[parameterName].Value = DBNull.Value;
            }
            else
            {
                this.SqlCommand.Parameters[parameterName].Value = value;
            }
        }

        public void AddObjectParameter(long objectId)
        {
            var sqlParameter = this.SqlCommand.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForObject;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.SqlCommand.Parameters.Add(sqlParameter);
        }

        public void AddTypeParameter(IClass @class)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForClass;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForClass;
            sqlParameter.Value = @class.Id;

            this.Parameters.Add(sqlParameter);
        }

        public void AddCountParameter(int count)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForCount;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForCount;
            sqlParameter.Value = count;

            this.Parameters.Add(sqlParameter);
        }

        public void AddCompositeRoleParameter(long objectId)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForCompositeRole;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.Parameters.Add(sqlParameter);
        }

        public void AddAssociationParameter(long objectId)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForAssociation;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.Parameters.Add(sqlParameter);
        }

        public void AddObjectTableParameter(IEnumerable<Reference> references)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.TableTypeNameForObject;
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new CompositesRoleDataRecords(this.Mapping, references);

            this.Parameters.Add(sqlParameter);
        }

        public void AddObjectTableParameter(IEnumerable<long> objectIds)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.TableTypeNameForObject;
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new ObjectDataRecord(this.Mapping, objectIds);
            ;

            this.Parameters.Add(sqlParameter);
        }

        public void AddCompositeRoleTableParameter(IEnumerable<CompositeRelation> relations)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = this.Mapping.TableTypeNameForCompositeRelation;
            sqlParameter.ParameterName = Mapping.ParamNameForTableType;
            sqlParameter.Value = new CompositeRoleDataRecords(this.Mapping, relations);

            this.Parameters.Add(sqlParameter);
        }

        public void AddAssociationTableParameter(long objectId)
        {
            var sqlParameter = this.CreateParameter();
            sqlParameter.ParameterName = Mapping.ParamNameForAssociation;
            sqlParameter.SqlDbType = Mapping.SqlDbTypeForObject;
            sqlParameter.Value = objectId;

            this.Parameters.Add(sqlParameter);
        }

        public object ExecuteScalar()
        {
            return this.SqlCommand.ExecuteScalar();
        }

        public void ExecuteNonQuery()
        {
            this.SqlCommand.ExecuteNonQuery();
        }

        public DataReader ExecuteReader() => new XDataReader(this.SqlCommand.ExecuteReader());

        public object GetValue(DataReader reader, int tag, int i)
        {
            switch (tag)
            {
                case UnitTags.Binary:
                    return reader.GetValue(i);

                case UnitTags.Boolean:
                    return reader.GetBoolean(i);

                case UnitTags.DateTime:
                    return reader.GetDateTime(i);

                case UnitTags.Decimal:
                    return reader.GetDecimal(i);

                case UnitTags.Float:
                    return reader.GetDouble(i);

                case UnitTags.Integer:
                    return reader.GetInt32(i);

                case UnitTags.String:
                    return reader.GetString(i);

                case UnitTags.Unique:
                    return reader.GetGuid(i);

                default:
                    throw new ArgumentException("Unknown Unit Tag: " + tag);
            }
        }
    }
}
