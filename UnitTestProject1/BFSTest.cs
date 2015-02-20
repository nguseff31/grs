using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GeomertyNetworkWorker.NetworkAnalyst;

namespace UnitTestProject1
{
    [TestClass]
    public class BFSTest
    {
        private InMemoryNetowork<int,int> network;
        private int GRS_CLASS = 1;
        private int ARMAT_CLASS = 2;
        private int NODE_CLASS = 3;

        [TestInitialize]
        public void initNetwork()
        {
            network = new InMemoryNetowork<int, int>();
            Dictionary<int, Vertex<int>> vs = new Dictionary<int, Vertex<int>>()
            {
                //Vertex<int>(id, class_id)
                { 1, new Vertex<int>(1, NODE_CLASS) },
                { 2, new Vertex<int>(2, NODE_CLASS) },
                { 3, new Vertex<int>(3, ARMAT_CLASS) },
                { 4, new Vertex<int>(4, GRS_CLASS) },
                { 5, new Vertex<int>(5, NODE_CLASS) },
                { 6, new Vertex<int>(6, GRS_CLASS) }
            };
            Dictionary<int, Edge<int>> es = new Dictionary<int, Edge<int>>()
            {
                //Edge<int>(id, fromVertexID, toVertexID, weigth)
                { 1, new Edge<int>(1, 2, 1, 1) },
                { 2, new Edge<int>(2, 3, 2, 1) },
                { 3, new Edge<int>(3, 4, 3, 1) },
                { 4, new Edge<int>(4, 5, 2, 1) },
                { 5, new Edge<int>(5, 6, 5, 1) }
            };
            int i = 0;
            network.loadFromData(vs, es);
        }

        /// <summary>
        /// Проверяем, что BFS посещает все вершины один раз, за исключением заблокированных задвижками
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void testBFSUpVisitAllVertexesExpectedBlocked()
        {
            HashSet<int> shouldBeVisited = new HashSet<int>()
            {
                1, 2, 5, 6
            };

            DirectedNetworkTracer.BFSUp<int, int>(network, 1,
                (vid) =>
                {
                    if (!shouldBeVisited.Contains(vid))
                    {
                        Assert.Fail("Вершина " + vid + 
                            " не должна быть посещена (или была посещена повторно)");
                    }
                    else
                    {
                        shouldBeVisited.Remove(vid);
                    }
                    return false;
                },
                (v, a) =>
                {
                    Vertex<int> vf = network.getVertexFeature(a);
                    //Если задвижка - дальше не идем
                    return vf.value != ARMAT_CLASS;
                });
            Assert.AreEqual<int>(shouldBeVisited.Count, 0, "Не все вершины были посещены");
        }

        /// <summary>
        /// Проверяем, что работает поиск
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void testBFSstopsAndReturnTarget()
        {
            //Ищем источник. 
            int grs = DirectedNetworkTracer.BFSUp<int, int>(network, 1,
                (vid) =>
                {
                    Vertex<int> vf = network.getVertexFeature(vid);
                    //Если vid - источник, то прекращаем выполнение
                    return vf.value == GRS_CLASS;
                },
                (v, a) =>
                {
                    Vertex<int> vf = network.getVertexFeature(a);
                    //Если vf - задвижка, то дальше не идем
                    return vf.value != ARMAT_CLASS;
                });
            //должна найтись вершина 6, но не 4 (т.к. перед ней задвижка)
            Assert.AreEqual(6, grs);
        }
    }
}
