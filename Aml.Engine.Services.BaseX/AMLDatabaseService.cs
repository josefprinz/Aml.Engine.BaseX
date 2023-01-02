using Aml.Engine.CAEX;
using Aml.Engine.Services.Interfaces;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Aml.Engine.Services.BaseX
{
    /// <summary>
    /// This service provides methods to connect to an AutomationML BaseX database
    /// and to query model data from the database.
    /// </summary>
    public class AMLDatabaseService: IAMLService
    {
        /// <summary>
        /// The rest client
        /// </summary>
        private HttpClient? _client;


        /// <summary>
        /// Establish a connection to the BaseX Rest server
        /// </summary>
        /// <param name="address">The server address 
        /// which is "http://localhost:8080/rest/" on a local computer</param>
        /// <param name="password">The user password</param>
        /// <param name="userName">The user name</param>
        /// <returns><c>true</c> when the server is running; <c>false</c> otherwise</returns>
        public bool Connect (string address, string userName, string password)
        {
            var handler = new HttpClientHandler 
            {
                Credentials = new NetworkCredential (userName, password)
            };
           
            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(address),
            };

            if (_client == null)
            {
                return false;
            }
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/xml"));
        
            return true;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AMLDatabaseService"/> class from being created.
        /// </summary>
        private AMLDatabaseService () { }

        /// <summary>
        /// Registers an instance of the AMLDatabaseService with
        /// the Aml.Engine <see cref="ServiceLocator"/>.
        /// </summary>
        /// <returns>The registered service.</returns>
        public static AMLDatabaseService Register ()
        {
            var service = new AMLDatabaseService ();    
            ServiceLocator.Register (service);
            return service;
        }

        private static XName Name (string tagName) => "{http://basex.org/rest}"+tagName;

        /// <summary>
        /// Gets the list of AutomationML documents, located in the named database.
        /// </summary>
        /// <param name="database">The name of the database.</param>
        /// <returns>A list of documents, defined by the document name and the document size in bytes.</returns>
        public async Task<List<(string Name, long Size)>> GetDocumentListAsync(string database)
        {
            List<(string Name, long Size)> documents = new();

            if (_client == null)
            {
                return documents;
            }

            using (var response = await _client.GetAsync($"{_client.BaseAddress}/{database}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var xContent = XDocument.Parse(result);
                    
                    foreach (var resource in xContent.Descendants(Name("resource")))
                    {
                        if (resource.Attribute("type")?.Value == "xml")
                        {
                            var sizeString = resource.Attribute("size")?.Value;
                            if (string.IsNullOrEmpty(sizeString))
                            {
                                continue;
                            }
                            var size = long.Parse(sizeString);
                            var name = resource.Value;

                            documents.Add((name, size));
                        }
                    }
                }
            }
            return documents;
        }

        /// <summary>
        /// Removes the registered instance of AMLDatabaseService from the
        /// Service registry.
        /// </summary>
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