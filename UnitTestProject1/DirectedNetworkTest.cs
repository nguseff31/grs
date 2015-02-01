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
        [TestMethod]
        public void testArmatAlg2()
        {
            yEdDirectedNetwork network = new yEdDirectedNetwork();
            network.setSymbols(this.symbols_to_classes);

            DirectedNetworkTracer tracer = new DirectedNetworkTracer(network);
            Dictionary<int, Vertex<CommonJunction>> vs = new Dictionary<int, Vertex<CommonJunction>>()
            {
                { 1, new Vertex<CommonJunction>(1, new CommonJunction(1, DirectedNetworkTracer.GRS_CLASS_ID, ""))},
                { 2, new Vertex<CommonJunction>(2, new CommonJunction(1, DirectedNetworkTracer.ARMAT_CLASS_ID, "")) },
                { 3, new Vertex<CommonJunction>(3, new CommonJunction(1, 0, "")) },
            };
            Dictionary<int, Edge<CommonJunction>> es = new Dictionary<int, Edge<CommonJunction>>()
            {
                { 1, new Edge<CommonJunction>(1, 1, 2, new CommonJunction()) },
                { 2, new Edge<CommonJunction>(2, 2, 3, new CommonJunction()) },
            };
            network.loadFromData(vs, es);
            Dictionary<int, Vertex<CommonJunction>> vs2 = new Dictionary<int, Vertex<CommonJunction>>(vs);
            Dictionary<int, Edge<CommonJunction>> es2 = new Dictionary<int, Edge<CommonJunction>>(es);
            vs2.Remove(1);
            es2.Remove(1);

            yEdDirectedNetwork result_network = new yEdDirectedNetwork();
            result_network.setSymbols(this.symbols_to_classes);

            result_network.loadFromData(vs2, es2);
            IDirectedNetwork<CommonJunction, CommonJunction> network2 = tracer.armTask(2).network;
            Assert.IsTrue(compareNetworks(result_network, network2));
        }

        [TestMethod]
        public void testArmatAlg()
        {
            yEdDirectedNetwork network = new yEdDirectedNetwork();
            network.setSymbols(this.symbols_to_classes);

            DirectedNetworkTracer tracer = new DirectedNetworkTracer(network);
            
            network.loadFromXml(this.getExample("test_arm"));

            yEdDirectedNetwork result_network = new yEdDirectedNetwork();
            result_network.setSymbols(this.symbols_to_classes);

            result_network.loadFromXml(this.getExample("test_arm_result"));
            IDirectedNetwork<CommonJunction, CommonJunction> network2 = tracer.armTask(12).network;
            Assert.IsTrue(compareNetworks(result_network, network2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void testArmatAlgFail()
        {
            yEdDirectedNetwork network = new yEdDirectedNetwork();
            network.setSymbols(this.symbols_to_classes);
            network.loadFromXml(this.getExample("test_arm_fail"));

            DirectedNetworkTracer tracer = new DirectedNetworkTracer(network);
            tracer.armTask(12);
        }
    }
}
