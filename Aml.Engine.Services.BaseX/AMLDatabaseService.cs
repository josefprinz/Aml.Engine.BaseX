using Aml.Engine.CAEX;
using Aml.Engine.Services.BaseX.Model;
using Aml.Engine.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace Aml.Engine.Services.BaseX
{
    /// <summary>
    /// This service provides methods to connect to an AutomationML BaseX database
    /// and to query model data from the database.
    /// </summary>
    public class AMLDatabaseService : IAMLService
    {
        /// <summary>
        /// The rest client
        /// </summary>
        private HttpClient? _client;

        private string? _error;

        public string ErrorMessage => _error ?? string.Empty;

        /// <summary>
        /// Establish a connection to the BaseX Rest server
        /// </summary>
        /// <param name="address">The server address
        /// which is "http://localhost:8080/rest/" on a local computer</param>
        /// <param name="password">The user password</param>
        /// <param name="userName">The user name</param>
        /// <returns>list of database information when the server is running; <c>null</c> otherwise null</returns>
        public async Task<IEnumerable<DatabaseInfo>> Connect(string address, string userName, string password)
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(userName, password)
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(address),
            };

            if (_client == null)
            {
                return Enumerable.Empty<DatabaseInfo>();
            }
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/xml"));

            _error = null;

            try
            {
                using var response = await _client.GetAsync($"{_client.BaseAddress}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var content = XDocument.Parse(result);
                    var databases = content.Descendants(Name("database"));

                    return databases.Select(d => new DatabaseInfo(d));
                }
                else
                {
                    _error = response.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
            return Enumerable.Empty<DatabaseInfo>();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="AMLDatabaseService"/> class from being created.
        /// </summary>
        private AMLDatabaseService()
        { }

        /// <summary>
        /// Registers an instance of the AMLDatabaseService with
        /// the Aml.Engine <see cref="ServiceLocator"/>.
        /// </summary>
        /// <returns>The registered service.</returns>
        public static AMLDatabaseService Register()
        {
            var service = new AMLDatabaseService();
            ServiceLocator.Register(service);
            return service;
        }

        private static XName Name(string tagName) => "{http://basex.org/rest}" + tagName;

        /// <summary>
        /// Gets the list of AutomationML documents, located in the named database.
        /// </summary>
        /// <param name="database">The name of the database.</param>
        /// <returns>A list of documents, defined by the document name and the document size in bytes.</returns>
        public async Task<IEnumerable<DocumentInfo>> GetDocumentListAsync(string database)
        {
            if (_client == null)
            {
                return Enumerable.Empty<DocumentInfo>();
            }

            try
            {
                using var response = await _client.GetAsync($"{_client.BaseAddress}/{database}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var xContent = XDocument.Parse(result);
                    var resources = xContent.Descendants(Name("resource")).Where (a=>a.Attribute("type")?.Value == "xml");

                    return resources.Select(r => new DocumentInfo(r));
                }
                else
                {
                    _error = response.StatusCode.ToString();
                }
            }

            catch (Exception ex)
            {
                _error = ex.Message;
            }

            return Enumerable.Empty<DocumentInfo>();
        }

        /// <summary>
        /// Loads the specified <paramref name="resource"/> from the
        /// <paramref name="database"/> as a new <see cref="CAEXDocument"/>.
        /// </summary>
        /// <remarks>
        /// Only the CAEXFile element and the root collections are loaded. All
        /// descendant elements are loaded on request only.
        /// </remarks>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// The loaded CAEXDocument if it exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<CAEXDocument?> LoadFromResourceAsync(string database, string resource)
        {
            if (_client == null)
            {
                return null;
            }

            using (var response = await _client.GetAsync($"{_client.BaseAddress}/{database}"))
            {
                if (response.IsSuccessStatusCode)
                {
                }
            }
            return null;
        }

        /// <summary>
        /// Removes the registered instance of AMLDatabaseService from the
        /// Service registry.
        /// </summary>
        public static void UnRegister()
        {
            var service = ServiceLocator.GetService<AMLDatabaseService>();
            if (service != null)
            {
                ServiceLocator.UnRegister<AMLDatabaseService>();
            }
        }
    }
}