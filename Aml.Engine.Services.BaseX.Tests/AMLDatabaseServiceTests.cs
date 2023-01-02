using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aml.Engine.Services.BaseX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Engine.Services.BaseX.Tests
{
    [TestClass()]
    public class AMLDatabaseServiceTests
    {
        [TestMethod()]
        public void ConnectTest()
        {
            Assert.Fail();
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
    }
}