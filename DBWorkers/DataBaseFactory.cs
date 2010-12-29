using System;
using System.Reflection;

namespace DBWorkers
{
	public sealed class DataBaseFactory
    {
        private DataBaseFactory() { }

        public static DataInterface CreateDatabase()
        {
            try
            {
                // Find the class as defined in the config file
                Type database = Type.GetType("DBWorkers.SqLite");

                // Get it's constructor
                ConstructorInfo constructor = database.GetConstructor(new Type[] { });

                // Invoke it's constructor, which returns an instance.
                DataInterface createdObject = (DataInterface)constructor.Invoke(null);	

                // Pass back the instance as a Database
                return createdObject;
            }
            catch (Exception excep)
            {
                throw new Exception("Error instantiating database. " + excep.Message);
            }
        }
    }
}