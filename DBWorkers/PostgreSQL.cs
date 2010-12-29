using System;
using Npgsql;
using System.Data;
using SupportClasses;

namespace DBWorkers
{
	public class PostgreSQL : DataInterface
	{
		public override IDbCommand CreateCommand()
		{
			return new NpgsqlCommand();
		}
        public override IDbConnection CreateConnection()
		{
			string connstring = MonitorConfiguration.DBConnString;
			
			NpgsqlConnection conn = new NpgsqlConnection(connstring);
			
			return conn;
		}
        public override IDbCommand CreateCommand(string commandText, IDbConnection connection)
		{
			return new NpgsqlCommand(commandText, (NpgsqlConnection)connection);
			
		}
		public override IDataAdapter CreateDataAdapter(string commandText, IDbConnection connection)
		{
			return new NpgsqlDataAdapter(commandText, (NpgsqlConnection)connection);			
		}
        public override IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection)
		{
			return null; 
		}
        public override IDataParameter CreateParameter(string parameterName, object parameterValue)
		{
			return new NpgsqlParameter(parameterName, parameterValue);
		}
		public override void DisposeAdapter(IDataAdapter ad)
		{
			NpgsqlDataAdapter da= (NpgsqlDataAdapter)ad;
			da.Dispose();
		}
	}
}

