using System;
using System.Data.SQLite;
using System.Data;
using System.Text.RegularExpressions;

namespace DBWorkers
{
	/// <summary>
	/// The SqLite implementation of the DataInterface
	/// Probably the best choice for the client side Data Base. 
	/// </summary>
	public class SqLite : DataInterface
	{		
		
		/// <summary>
		/// Gets a connection to the local database
		/// </summary>
		/// <param name="conn">
		/// A <see cref="System.Object"/> containing the connection to the sqlite database (SQLiteConnection).
		/// </param>
		public override IDbConnection CreateConnection()
		{			
			IDbConnection conn;
			
			try{
                conn = new SQLiteConnection("data source=|DataDirectory|TorrentNerd.sqlite;version=3;default timeout=5;journal mode=Persist;useutf16encoding=True;pooling=False");					
			}
			catch(Exception e)
			{
				throw new Exception(e.ToString(), e);
			}
			
			return conn;
		}
		
		public override IDbCommand CreateCommand()
        {
            return new SQLiteCommand();
        }		
		
		public override IDbCommand CreateStoredProcCommand (string procName, IDbConnection connection)
		{
			throw new NotImplementedException ();
		}

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection)
        {
            SQLiteCommand command = (SQLiteCommand)CreateCommand();

            command.CommandText = commandText;
            command.Connection = (SQLiteConnection)connection;
            command.CommandType = CommandType.Text;

            return command;
        }

        public override IDataParameter CreateParameter(string parameterName, object parameterValue)
        {
            return new SQLiteParameter(parameterName, parameterValue);
        }	
		
		public override IDataAdapter CreateDataAdapter(string commandText, IDbConnection connection)
        {
			return new SQLiteDataAdapter(commandText, (SQLiteConnection)connection);
        }	
		
		public override void DisposeAdapter(IDataAdapter ad)
        {
			SQLiteDataAdapter da = (SQLiteDataAdapter)ad;
			da.Dispose();
        }
	}
}

