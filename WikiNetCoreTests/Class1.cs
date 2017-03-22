using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WikiNetCoreTests
{
    [TestFixture]
    public class MyTestFixture
    {
        [Test]
        public void MyTest()
        {
            Assert.That("A", Is.EqualTo("b"));
        }
    }
}
