using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCPClient;

namespace CommandTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CommandTest()
        {
            Command command = new Command("command:parameter");
            Assert.AreEqual("command", command.command);
            Assert.AreEqual("parameter", command.parameter);
        }
    }
}
