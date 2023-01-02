using Aml.Engine.Services.Interfaces;

namespace Aml.Engine.Services.BaseX
{
    /// <summary>
    /// This service provides methods to connect to an AutomationML BaseX database
    /// and to query model data from the database.
    /// </summary>
    public class AMLDatabaseService: IAMLService
    {
        /// <summary>
        /// Establish a database connection
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect (string host, string port)
        {
            return false;
        }

        private AMLDatabaseService () { }

        public static AMLDatabaseService Register ()
        {
            var service = new AMLDatabaseService ();    
            ServiceLocator.Register (service);
            return service;
        }

        public static void UnRegister ()
        {
            var service = ServiceLocator.GetService<AMLDatabaseService> ();
            if (service != null)
            { 
                ServiceLocator.UnRegister<AMLDatabaseService>();
            }
        }
    }
}