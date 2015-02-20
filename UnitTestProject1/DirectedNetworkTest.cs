using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeomertyNetworkWorker.NetworkAnalyst;
using GeomertyNetworkWorker;

namespace UnitTestProject1
{
    /// <summary>
    /// Интеграционные тесты для yEdNetwork
    /// </summary>
    [TestClass]
    public class DirectedNetworkTest : ExampleTest
    {
        private yEdDirectedNetwork network;

        [TestInitialize]
        public void InitNetwork()
        {
            network = new yEdDirectedNetwork();

            network.setSymbols(this.symbols_to_classes);
            network.loadFromXml(this.getExample("test"));
        }
        protected override string getRelativePath()
        {
            return "";
        }
        [TestMethod]
        [TestCategory("Integration")]
        public void checkDirectory()
        {
            Assert.IsTrue(Directory.Exists(getFixturePath()));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void vertexesAndEdgesLoaded()
        {
            Assert.AreEqual<int>(9, network.Vertexes.Count());
            Assert.AreEqual<int>(9, network.Edges.Count());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void getAdjacentTest()
        {
            Assert.IsTrue(compareCollections(new int[] { 2, 3, 4 }, network.getAdjacentEdges(3)));

            Assert.IsTrue(compareCollections(new int[] { 4, 5 }, network.getSuccessors(3)));
            Assert.IsTrue(compareCollections(new int[] { 2 }, network.getPredecessors(3)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void getFeatureTest()
        {
            Assert.AreEqual(network.getEdgeFeature(1).GetType(), typeof(Edge<CommonJunction>));
        }
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        [TestCategory("UnitTest")]
        public void getFeatureFailTest()
        {
            var edge = network.getEdgeFeature(16);
        }
        [TestMethod]
        [TestCategory("UnitTest")]
        public void containsTest()
        {
            Assert.IsTrue(network.containsEdge(1, 2));
            Assert.IsFalse(network.containsEdge(2, 1));
            Assert.IsTrue(network.containsVertex(1));
            Assert.IsFalse(network.containsVertex(15));
        }
    }
}
