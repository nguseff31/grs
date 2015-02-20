using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using GeomertyNetworkWorker.NetworkAnalyst;
using GeomertyNetworkWorker;

namespace UnitTestProject1
{
    [TestClass]
    public class NetworkAnalystTest : ExampleTest
    {
        private yEdDirectedNetwork network;

        protected override string getRelativePath()
        {
            return "";
        }

        [TestInitialize]
        public void initInMemoryNetwork() 
        {
            network = new yEdDirectedNetwork();
            network.setSymbols(this.symbols_to_classes);
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
        }

        private bool testArmTask(IDirectedNetwork<CommonJunction,CommonJunction> network, IDirectedNetwork<CommonJunction,CommonJunction> expected_result, int edgeID) 
        {
            DirectedNetworkTracer tracer = new DirectedNetworkTracer(network);
            IDirectedNetwork<CommonJunction, CommonJunction> result = tracer.armTask(edgeID).network;
            return compareNetworks(result, expected_result);
        }

        private bool testArmExample(string name, int EdgeID) 
        {
            yEdDirectedNetwork network = new yEdDirectedNetwork();
            network.setSymbols(this.symbols_to_classes);
            network.loadFromXml(this.getExample(name));
            DirectedNetworkTracer tracer = new DirectedNetworkTracer(network);

            yEdDirectedNetwork expected_result = new yEdDirectedNetwork();
            expected_result.setSymbols(this.symbols_to_classes);

            expected_result.loadFromXml(this.getExample(name + "_result"));

            IDirectedNetwork<CommonJunction, CommonJunction> result = tracer.armTask(EdgeID).network;
            return compareNetworks(result, expected_result);
        }
        [TestMethod]
        [TestCategory("Functional")]
        public void testArmatAlgInMemory()
        {
            Dictionary<int, Vertex<CommonJunction>> vs2 = new Dictionary<int, Vertex<CommonJunction>>(network.Vertexes.ToDictionary((x) => x.id));
            Dictionary<int, Edge<CommonJunction>> es2 = new Dictionary<int, Edge<CommonJunction>>(network.Edges.ToDictionary((x) => x.id));
            vs2.Remove(1);
            es2.Remove(1);

            yEdDirectedNetwork result_network = new yEdDirectedNetwork();
            result_network.setSymbols(this.symbols_to_classes);

            result_network.loadFromData(vs2, es2);
            Assert.IsTrue(testArmTask(network, result_network, 2));
        }

        [TestMethod]
        [TestCategory("Functional")]
        public void testArmatAlg()
        {
            string[] examples = new string[] { "test_arm" };
            int[] edges = new int[] { 12 };

            for(int i = 0; i < examples.Length; i++) 
            {
                Assert.IsTrue(testArmExample(examples[i], edges[i]), "Ошибка в примере " + examples[i]);
            }
        }

        [TestMethod]
        [TestCategory("Functional")]
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
