using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeomertyNetworkWorker.NetworkAnalyst;
using GeomertyNetworkWorker;

namespace UnitTestProject1
{
    [TestClass]
    public class DirectedNetworkTest : ExampleTest
    {
        protected override string getRelativePath()
        {
            return "";
        }
        [TestMethod]
        public void checkDirectory()
        {
            Assert.IsTrue(Directory.Exists(getFixturePath()));
        }

        [TestMethod]
        public void loadingExample()
        {
            yEdDirectedNetwork network = new yEdDirectedNetwork();
            network.setSymbols(this.symbols_to_classes);

            network.loadFromXml(this.getExample("test"));

            int count = network.Vertexes.Count();

            Assert.AreEqual<int>(9, count);

            Assert.IsTrue(compareCollections(new int[] { 2, 3, 4 }, network.getAdjacentEdges(3)));

            Assert.IsTrue(compareCollections(new int[] { 4, 5 }, network.getSuccessors(3)));
            Assert.IsTrue(compareCollections(new int[] { 2 }, network.getPredecessors(3)));
            Assert.IsTrue(network.containsEdge(1, 2));
            Assert.IsFalse(network.containsEdge(2, 1));
        }
    }
}
