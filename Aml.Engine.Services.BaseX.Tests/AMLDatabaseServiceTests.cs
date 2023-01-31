using Aml.Engine.CAEX;
using Aml.Engine.Services.BaseX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Aml.Engine.Services.BaseX.Tests
{
    [TestClass()]
    public class AMLDatabaseServiceTests
    {
        private AMLDatabaseService? _service;

        [TestInitialize()]
        public void InitTests()
        {
            _service = AMLDatabaseService.Register();
            var _ = _service.Connect("http://localhost:8080/rest/", "admin", "josef").Result;
        }

        [TestMethod()]
        public void ConnectTest()
        {
            Assert.IsTrue(_service?.Connect("http://localhost:8080/rest/", "admin", "josef").Result.Any());
        }

        [TestMethod()]
        public void RegisterTest()
        {
            var service = AMLDatabaseService.Register();
            Assert.IsNotNull(service);
            Assert.IsNotNull(Aml.Engine.Services.ServiceLocator.GetService<AMLDatabaseService>());
        }

        [TestMethod()]
        public void UnRegisterTest()
        {
            AMLDatabaseService.UnRegister();
            Assert.IsNull(Aml.Engine.Services.ServiceLocator.GetService<AMLDatabaseService>());
        }

        [TestMethod()]
        public void GetDocumentListAsyncTest()
        {
            var documents = _service?.GetDocumentListAsync("AutomationML").Result;
            Assert.IsTrue(documents?.Count() > 0);
        }

        [TestMethod()]
        public void LoadCAEXFileHeaderAsXDocumentAsyncTest()
        {
            var document = _service?.LoadCAEXFileHeaderAsXDocumentAsync("AutomationML", "AssetAdministrationShellLib.aml").Result;
            Assert.IsTrue(document is not null);
        }

        [TestMethod()]
        public void PostExampleTest()
        {
            var text = _service?.PostExample().Result;
            Assert.IsNotNull(text);
        }



        [TestMethod()]
        public void ElementsTest()
        {
            var document = _service?.LoadCAEXFileHeaderAsXDocumentAsync("AutomationML", "AssetAdministrationShellLib.aml").Result;
            
            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);

            var roleClassLibraries = _service?.Elements (document.Root, CAEX_CLASSModel_TagNames.ROLECLASSLIB_STRING);
            Assert.IsTrue(roleClassLibraries?.Any());

            _service?.Elements(document.Root, CAEX_CLASSModel_TagNames.ROLECLASSLIB_STRING, true);
            Assert.IsTrue(document.Root.Elements(CAEX_CLASSModel_TagNames.ROLECLASSLIB_STRING).Any());

        }


        [TestMethod()]
        public void LoadCAEXFileHeaderAsCAEXDocumentAsyncTest()
        {
            var document = _service?.LoadCAEXFileHeaderAsCAEXDocumentAsync("AutomationML", "AssetAdministrationShellLib.aml").Result;
            Assert.IsTrue(document is CAEXDocument doc && !doc.CAEXFile.RoleClassLib.Any());
        }


        [TestMethod()]
        public void LoadCAEXDocumentAsyncTest()
        {
            var document = _service?.LoadCAEXDocumentAsync("AutomationML", "AssetAdministrationShellLib.aml").Result;
            Assert.IsTrue(document is CAEXDocument doc && doc.CAEXFile.RoleClassLib.Any());
        }

        [TestMethod()]
        public void LoadXDocumentAsyncTest()
        {
            var document = _service?.LoadXDocumentAsync("AutomationML", "AssetAdministrationShellLib.aml").Result;
            Assert.IsTrue(document is XDocument doc && doc.Descendants(CAEX_CLASSModel_TagNames.ROLECLASSLIB_STRING).Any());
        }


        [TestMethod()]
        public void RemoveDocumentTest()
        {
            using ( var document = _service?.LoadCAEXDocumentAsync("AutomationML", "AssetAdministrationShellLib.aml").Result)
            {
                Assert.IsTrue(_service?.IsLoaded("AutomationML", "AssetAdministrationShellLib.aml"));
            }
            Assert.IsFalse(_service?.IsLoaded("AutomationML", "AssetAdministrationShellLib.aml"));
        }
    }
}