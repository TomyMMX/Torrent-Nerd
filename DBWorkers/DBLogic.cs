using System;
using System.Collections;
using System.Data;
using System.Threading;
using log4net;
using System.Collections.Generic;

namespace DBWorkers
{
	public class DBLogic : DBWorker
	{		
		private static readonly ILog MonitorLog = LogManager.GetLogger(typeof(DBLogic));
		
		//client side DB initialization
		private static readonly string clientInitCommand = 
			"CREATE TABLE config(torrentapp varchar(255), host varchar(255), port varchar(255), savelocation varchar(255));"+
			"CREATE TABLE library(name varchar(255), type integer, ep integer, season integer, year integer, imdblink varchar(255), imdbrating double, imdbplot text, posterlink varchar(255), downloaded boolean,"+
				                 "repacked boolean, waswatched boolean, pathtoplay varchar(255), sublang integer, idoftorrent varchar(255));"+
			"CREATE TABLE suggestion(name varchar(255), year integer, imdblink varchar(255), posterlink varchar(255), rating integer, torrentlink varchar(255), removed boolean, downloaded boolean);"+
			"INSERT INTO config(torrentapp, host, port, savelocation) VALUES ('transmission', '127.0.0.1', '9091', '');";
		
		//non query statements
		private static readonly string insertToLibCommand = 
			"INSERT INTO library (name, type, ep, season, year, imdblink, imdbrating, imdbplot, posterlink, downloaded, repacked, waswatched, pathtoplay, sublang, idoftorrent)"+
				" VALUES (@name, @type, @ep, @season, @imdblink, @imdbrating, @imdbplot, @posterlink, 'false', 'false', 'false', @pathtoplay, @sublang, @idoftorrent);";
				
        private static readonly string setLastTimeStamp =
			"UPDATE machine SET lasttimestamp = @timestamp2 WHERE id = @id";		
				
		//query statement
		private static readonly string getConfigCommand =
			"SELECT torrentapp, host, port, savelocation FROM config WHERE ROWID = 1";
		
		
		
		/// <summary>
		///A function that is used by the clients to initialize a new empty DB 
		/// </summary>
		public static void createSqliteSchema()
		{			
				makeNonQuery(false, clientInitCommand, null);
				MonitorLog.Info("Initialized new client DB");			
		}		
		
		/// <summary>
		///Creates a IDataParameter with the specified settings. 
		/// </summary>
		/// <param name="paramname">
		/// A <see cref="System.String"/> that represents the parameter name.
		/// </param>
		/// <param name="paramValue">
		/// A <see cref="System.Object"/> that holds the parameter value.
		/// </param>
		/// <returns>
		/// A <see cref="IDataParameter"/>
		/// </returns>
		private static IDataParameter getparameter(string paramname, object paramValue)
		{
			IDataParameter p;
			p = database.CreateParameter(paramname, paramValue);
			
			return p;
		}
	
		public static void InsertToLib(string name, int type, int ep, int season, int year, string imdblink, double imdbrating, string imdbplot, string posterlink, string pathtoplay, string sublang, string idoftorrent)
        {
            ArrayList parameters = new ArrayList();

            parameters.Add(getparameter("@name", name));
            parameters.Add(getparameter("@type", type));
			parameters.Add(getparameter("@ep", ep));
			parameters.Add(getparameter("@season", season));
			parameters.Add(getparameter("@year", year));
			parameters.Add(getparameter("@imdblink", imdblink));
			parameters.Add(getparameter("@imdbrating", imdbrating));
			parameters.Add(getparameter("@imdbplot", imdbplot));
			parameters.Add(getparameter("@posterlink", posterlink));
			parameters.Add(getparameter("@pathtoplay", pathtoplay));
			parameters.Add(getparameter("@sublang", sublang));
			parameters.Add(getparameter("@idoftorrent", idoftorrent));			

            makeNonQuery(false, insertToLibCommand, parameters);
        }
		
        
		public static string[] getConfig()
		{
			DataTable DT = new DataTable();
			string command = getConfigCommand;
			makeNewQuery(command, out DT);
			//"SELECT torrentapp, host, port, savelocation FROM machine WHERE ROWID = 1";
			string [] configs= new string[4];
 			configs[0] = DT.Rows[0][0].ToString();
			configs[1] = DT.Rows[0][1].ToString();
			configs[2] = DT.Rows[0][2].ToString();
			configs[3] = DT.Rows[0][3].ToString();
			
			return configs;			
		}
		

		
		/// <summary>
		/// Inserts or updates a row in a table 
		/// </summary>
		/// <param name="retry">
		/// A <see cref="System.Boolean"/>
		/// </param>
		/// <param name="command">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="parameters">
		/// A <see cref="ArrayList"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> that represents the number of row affected by the command
		/// </returns>
		public static int makeNonQuery(bool retry, string command, ArrayList parameters)
		{		
			int rowsAffected = 0;
			try
			{					
				if(retry)
				{
					Thread.Sleep(new Random().Next(250,500)); 
					MonitorLog.Debug("Trying to resolve DB lock!");
				}	
				
				using (IDbConnection conn = database.CreateConnection())
    			{
					conn.Open();
        			using (IDbCommand cmd = database.CreateCommand(command, conn))
        			{	
						if(parameters!=null)
						{
							foreach(IDataParameter p in parameters)
							{
								cmd.Parameters.Add(p);							
							}					
						}
						rowsAffected = cmd.ExecuteNonQuery();
						
						cmd.Cancel();
						cmd.Dispose();
						
						if(retry)
							MonitorLog.Debug("Succesfuly resolved previous DB lock!");
				
						conn.Close();
        			}
    			}
			}
			
			catch(Exception ex)
			{	
				if(ex.ToString().Contains(" lock"))
				{
					if(!retry)
						MonitorLog.Debug("DB locked waiting for resources to free!", ex);
					
					makeNonQuery(true, command, parameters);	
				}
				else
					MonitorLog.Error(string.Format("Problems whit SQL statement: {0}", command), ex);
					throw ex;
			}	
			
			return rowsAffected;
		}
		
		public static void makeNewQuery (string command, out DataTable DT)
		{	
			DT = null;	
			try{					
				using (IDbConnection conn = database.CreateConnection())
    			{
					conn.Open();
        			IDataAdapter DB = database.CreateDataAdapter(command,conn);					
					DataSet DS= new DataSet();
					DS.Reset(); 
					DB.Fill(DS); 				
					DT = DS.Tables[0];	
					database.DisposeAdapter(DB);
					conn.Close();		
				}
			}
			catch(Exception e)
			{
				MonitorLog.Error(string.Format("Problems whit SQL QUERY statement: {0}", command), e);
				throw e;
			}
			
			
		}
	}
}