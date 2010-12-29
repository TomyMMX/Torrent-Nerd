using System;
using System.Data;

namespace DBWorkers
{
	/// <summary>
	/// Method that every database clas should implement for their specific DB type
	/// </summary>
	public abstract class DataInterface
	{	
        public abstract IDbCommand CreateCommand();
        public abstract IDbConnection CreateConnection();
        public abstract IDbCommand CreateCommand(string commandText, IDbConnection connection);
		public abstract IDataAdapter CreateDataAdapter(string commandText, IDbConnection connection);
        public abstract IDbCommand CreateStoredProcCommand(string procName, IDbConnection connection);
        public abstract IDataParameter CreateParameter(string parameterName, object parameterValue);
		public abstract void DisposeAdapter(IDataAdapter ad);
	}
}

