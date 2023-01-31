using Aml.Engine.CAEX;
using Aml.Engine.Services.AML;
using Aml.Engine.Services.BaseX.Helper;
using Aml.Engine.Services.BaseX.Model;
using Aml.Engine.Services.Interfaces;
using Aml.Engine.Xml.Extensions;
using Aml.Engine.XML;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace Aml.Engine.Services.BaseX
{
    /// <summary>
    /// This service provides methods to connect to an AutomationML BaseX database
    /// and to query model data from the database.
    /// </summary>
    public class AMLDatabaseService : IDatabaseService, IXMLDocumentRegistry
    {
        #region Fields

        private readonly DocumentReferenceDictionary<DocumentInfo> _documentTable;

        /// <summary>
        /// The rest client
        /// </summary>
        private HttpClient? _client;

        private string? _error;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="AMLDatabaseService"/> class from being created.
        /// </summary>
        private AMLDatabaseService()
        {
            _documentTable = new();
        }

        #endregion Constructors

        #region Properties

        public string ErrorMessage => _error ?? string.Empty;

        #endregion Properties

        #region Methods

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
                service.Unload();
            }
        }

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
            // define the credentials for the BaseX server
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(userName, password)
            };

            // create the HttpClient with server address (local address is http://localhost:8080/rest/)
            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri(address),
            };

            // get the result as XML
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/xml"));

            _error = null;

            // check, if the server is running by getting the list of databases
            try
            {
                using var response = await _client.GetAsync($"{_client.BaseAddress}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    // parse the XML result as an XDocument
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
        /// Returns a filtered collection of the child elements of the specified element
        /// in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are
        /// included in the collection.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
        /// <remarks>
        /// If the specified element doesnot contains children with the specified name the
        /// database is queried, otherwise the loaded children collection is returned.
        /// </remarks>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> containing
        /// the children of the <see cref="T:System.Xml.Linq.XElement" /> that have a matching <see cref="T:System.Xml.Linq.XName" />,
        /// in document order.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<XElement> Elements(XElement element, XName name, bool insert = false)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (element.Elements(name).Any())
            {
                return element.Elements(name);
            }

            if (!_documentTable.IsLoaded(element.Document))
            {
                return Enumerable.Empty<XElement>();
            }

            string? path = null;
            var documentInfo = _documentTable[element.Document];
            switch (name.LocalName)
            {
                case CAEX_CLASSModel_TagNames.ROLECLASSLIB_STRING:
                case CAEX_CLASSModel_TagNames.EXTERNALREFERENCE_STRING:
                case CAEX_CLASSModel_TagNames.ATTRIBUTETYPELIB_STRING:
                case CAEX_CLASSModel_TagNames.INTERFACECLASSLIB_STRING:
                case CAEX_CLASSModel_TagNames.INSTANCEHIERARCHY_STRING:
                case CAEX_CLASSModel_TagNames.SYSTEMUNITCLASSLIB_STRING:
                    break;

                default:
                    path = element.GetAbsoluteXPath();
                    break;
            }

            var elementsString = Task.Run(() => GetElementHeaderAsync(documentInfo.DatabaseName, documentInfo.Name, name.LocalName, path));

            if (string.IsNullOrEmpty(elementsString.Result))
            {
                return Enumerable.Empty<XElement>();
            }
            var doc = XDocument.Parse(elementsString.Result);
            if (doc != null)
            {
                var elements = (doc.Element("XElements")) is XElement e
                    ? e.Elements()
                    : Enumerable.Empty<XElement>();

                if (insert)
                {
                    var caexParent = element.CreateCAEXWrapper() as CAEXBasicObject;
                    bool isInserted = false;
                    if (caexParent != null)
                    {
                        foreach (var child in elements)
                        {
                            var caexChild = child.CreateCAEXWrapper() as CAEXBasicObject;
                            if (caexChild != null)
                            {
                                isInserted = caexParent.Insert(caexChild, false);
                            }
                        }
                    }

                    if (isInserted)
                    {
                        return element.Elements(name);
                    }
                }
                return elements;
            }
            return Enumerable.Empty<XElement>();
        }

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
                    var resources = xContent.Descendants(Name("resource")).Where(a => a.Attribute("type")?.Value == "xml");

                    return resources.Select(r => new DocumentInfo(database, r));
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

        public async Task<string> RunQueryAsync(string database, string query)
        {
            if (_client == null)
            {
                return string.Empty;
            }

            try
            {
                return await RunXQueryAsync(database, query);
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Loads the CAEXFile root node from the specified <paramref name="resource"/> from the
        /// <paramref name="database"/> as a new <see cref="CAEXDocument"/>.
        /// </summary>
        /// <remarks>
        /// Only the CAEXFile element and the header information is loaded. All
        /// descendant elements are loaded on request only.
        /// </remarks>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// The loaded CAEXDocument if it exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<CAEXDocument?> LoadCAEXFileHeaderAsCAEXDocumentAsync(string database, string resource)
        {
            try
            {
                var nodeString = await GetElementHeaderAsync(database, resource, CAEX_CLASSModel_TagNames.CAEX_FILE);
                if (!string.IsNullOrEmpty(nodeString))
                {
                    var document = await CAEXDocument.LoadFromStringAsync(nodeString);
                    if (document == null)
                    {
                        _error = $"Cannot load CAEXDocument from resource {resource}";
                        return null;
                    }
                    _documentTable.Add(document.XDocument, new DocumentInfo(database, resource));
                    return document;
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// Loads the CAEXFile header of the specified <paramref name="resource"/> from the
        /// <paramref name="database"/> as a new <see cref="XDocument"/>.
        /// </summary>
        /// <remarks>
        /// Only the CAEXFile element and the header information is loaded. All
        /// other descendant elements are loaded on request only.
        /// </remarks>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// An XDocument if exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<XDocument?> LoadCAEXFileHeaderAsXDocumentAsync(string database, string resource)
        {
            try
            {
                var nodeString = await GetElementHeaderAsync(database, resource, CAEX_CLASSModel_TagNames.CAEX_FILE);
                if (!string.IsNullOrEmpty(nodeString))
                {
                    var document = XDocument.Parse(nodeString);
                    if (document == null)
                    {
                        _error = $"Cannot create XDocument from {resource}";
                        return null;
                    }
                    _documentTable.Add(document, new DocumentInfo(database, resource));
                    return document;
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// Loads the CAEXFile from the specified <paramref name="resource"/> from the
        /// <paramref name="database"/> as a new <see cref="CAEXDocument"/>.
        /// </summary>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// The loaded CAEXDocument if it exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<CAEXDocument?> LoadCAEXDocumentAsync(string database, string resource)
        {
            try
            {
                var nodeString = await LoadCAEXFileAsync(database, resource);
                if (!string.IsNullOrEmpty(nodeString))
                {
                    var document = await CAEXDocument.LoadFromStringAsync(nodeString);
                    if (document == null)
                    {
                        _error = $"Cannot load CAEXDocument from resource {resource}";
                        return null;
                    }
                    _documentTable.Add(document.XDocument, new DocumentInfo(database, resource));
                    return document;
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// Loads the CAEXFile of the specified <paramref name="resource"/> from the
        /// <paramref name="database"/> as a new <see cref="XDocument"/>.
        /// </summary>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// An XDocument if exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<XDocument?> LoadXDocumentAsync(string database, string resource)
        {
            try
            {
                var nodeString = await LoadCAEXFileAsync(database, resource);
                if (!string.IsNullOrEmpty(nodeString))
                {
                    var document = XDocument.Parse(nodeString);
                    if (document == null)
                    {
                        _error = $"Cannot create XDocument from {resource}";
                        return null;
                    }
                    _documentTable.Add(document, new DocumentInfo(database, resource));
                    return document;
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// Posts an xquery to the example database named factbook.
        /// Used to text xquery requests.
        /// </summary>
        /// <returns></returns>
        public async Task<string?> PostExample()
        {
            if (_client == null)
            {
                return null;
            }

            Uri url = new($"{_client.BaseAddress}/factbook");

            String request =
       "<query xmlns='http://basex.org/rest'>\n" +
       "  <text>//city/@*</text>\n" +
       "</query>";

            var stringContent = new StringContent(request, Encoding.UTF8, "application/query+xml");

            try
            {
                using var response = await _client.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// Removes the document from the internal registry of the service.
        /// </summary>
        /// <param name="document">The XML document.</param>
        public void RemoveDocument(XDocumentWrapper document)
        {
            //ToDo provide action when document is removed
            _documentTable.RemoveFromTable(document.XDocument);
        }

        public bool IsLoaded(string database, string resource)
        {
            return _documentTable.Values.Any(d => d.DatabaseName == database && d.Name == resource);
        }

        private static XName Name(string tagName) => "{http://basex.org/rest}" + tagName;

        private static string HeaderRequestString(string database, string resource, string elementName, string? path = null)
        {
            return path == null
                ?
                "<run>\n" +
                $"<variable name='file' value='{database}/{resource}'/>\n" +
                $"<text>{elementName}Header.xq</text>\n" +
                "</run>"
                :
                "<run>\n" +
                $"<variable name='file' value='doc('{database}/{resource}'){path}'/>\n" +
                $"<text>{elementName}Header.xq</text>\n" +
                "</run>";
        }

        /// <summary>
        /// Loads the CAEXFile document root node from the specified <paramref name="resource"/> from the
        /// <paramref name="database"/>.
        /// </summary>
        /// <remarks>
        /// Only the CAEXFile element and the header information is loaded.
        /// </remarks>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// The root node string if exists; otherwise <c>null</c>.
        /// </returns>
        private async Task<string> GetElementHeaderAsync(string database, string resource, string name, string? path = null)
        {
            if (_client == null)
            {
                return string.Empty;
            }

            Uri url = new($"{_client.BaseAddress}{database}");

            string request = HeaderRequestString(database, resource, name, path);

            var stringContent = new StringContent(request, Encoding.UTF8, "application/query+xml");

            try
            {
                using var response = await _client.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                    {
                        _error = "no content";
                        return string.Empty;
                    }
                    return result;
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
            return string.Empty;
        }

        /// <summary>
        /// Loads the CAEXFile document from the specified <paramref name="resource"/> from the
        /// <paramref name="database"/>.
        /// </summary>
        /// <param name="database">The name of the AutomationML Database</param>
        /// <param name="resource">The name of the AML resource file</param>
        /// <returns>
        /// The CAEXFile as string if exists; otherwise <c>string.Empty</c>.
        /// </returns>
        private async Task<string> LoadCAEXFileAsync(string database, string resource)
        {
            if (_client == null)
            {
                return string.Empty;
            }

            Uri url = new($"{_client.BaseAddress}{database}");

            string request = "<query>\n" +
                $"<text>doc('{database}/{resource}')</text>\n" +
                "</query>";

            var stringContent = new StringContent(request, Encoding.UTF8, "application/query+xml");

            try
            {
                using var response = await _client.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                    {
                        _error = "no content";
                        return string.Empty;
                    }
                    return result;
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
            return string.Empty;
        }

        private async Task<string> RunXQueryAsync(string database, string query)
        {
            if (_client == null)
            {
                return string.Empty;
            }

            Uri url = new($"{_client.BaseAddress}{database}");

            string request = "<query>\n" +
                $"<text>{query}</text>\n" +
                "</query>";

            //request = "<query>\n" +
            //    "<text>for $i in .//text() return string-length($i)</text>\n" +
            //    "<context>\n" +
            //    "<xml>\n" +
            //        "<text>Hello</text>\n" +
            //        "<text>World</text>\n" +
            //    "</xml>\n" +
            //    "</context>\n" +
            //    "</query>";

            var stringContent = new StringContent(request, Encoding.UTF8, "application/query+xml");

            try
            {
                using var response = await _client.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                    {
                        _error = "no content";
                        return string.Empty;
                    }
                    return result;
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
            return string.Empty;
        }

        private void Unload()
        {
            _documentTable.Clear();
        }

        #endregion Methods
    }
}