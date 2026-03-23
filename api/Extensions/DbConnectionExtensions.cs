using System.Data;

namespace Extensions
{
    public static class DbConnectionExtensions
    {
        public static IDbTransaction BeginSerializableTransaction(this IDbConnection dbConnection)
        {
            // Dapper takes care of the connection for other queries, 
            // but using ADO transactions we need to open it if closed
            if(dbConnection.State is ConnectionState.Closed)
                dbConnection.Open();

            return dbConnection.BeginTransaction(IsolationLevel.Serializable);
        }
    }
}
