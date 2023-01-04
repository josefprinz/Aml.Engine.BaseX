using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}