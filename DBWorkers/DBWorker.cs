using System;
using System.Data;
namespace DBWorkers
{
	public class DBWorker
    {
        private static DataInterface _database = null;
		
        static DBWorker()
        {
            try
            {
                _database = DataBaseFactory.CreateDatabase();
            }
            catch (Exception excep)
            {
                throw excep;
            }
        }

        public static DataInterface database
        {
            get { return _database; }
        }
    }
}