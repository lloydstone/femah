using System.Data.SqlClient;
using System.Data.Common;
using System;

namespace Femah.Core.Providers
{
    public interface ISqlCommand
    {
        DbDataReader ExecuteReader();
        void ExecuteNonQuery();
        Object ExecuteScalar();
        void AddParameter(SqlParameter param);
    }
}