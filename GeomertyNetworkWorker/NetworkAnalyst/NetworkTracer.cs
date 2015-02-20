using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeomertyNetworkWorker.NetworkAnalyst
{
    public class DirectedNetworkTracer
    {
        public IDirectedNetwork<CommonJunction, CommonJunction> network;
        public const int GRS_CLASS_ID = 1;
        public const int ABON_CLASS_ID = 2;
        public const int ARMAT_CLASS_ID = 3;

        public DirectedNetworkTracer(IDirectedNetwork<CommonJunction, CommonJunction> network)
        {
            this.network = network;
        }
        /// <summary>
        /// Обход в ширину (вниз, по направлению к источнику)
        /// </summary>
        /// <param name="start">Начало обхода</param>
        /// <param name="processVertex">(int vertexID) => bool. Если возвращает false, то обход заканчивается и BFS возвращает vertexID </param>
        /// <param name="processAdjacent">(v, a) => bool. Если возвращает false, то вершина a не добавляется в очередь и обход в направлении v->a не идет</param>
        /// <returns>Точка, на которой законечен обход</returns>
        public static int BFSUp<TVertex, TEdge>(IDirectedNetwork<TVertex, TEdge> network, int start, Func<int, bool> processVertex, Func<int,int,bool> processAdjacent) 
        {
            int current = start;
            HashSet<int> visited = new HashSet<int>();

            Queue<int> q = new Queue<int>();
            q.Enqueue(start);
            visited.Add(start);

            while (q.Count != 0)
            {
                current = q.Dequeue();

                if(processVertex(current))
                    return current;

                foreach (int adj in network.getPredecessors(current))
                {
                    if (!visited.Contains(adj))
                    {
                        if(processAdjacent(current, adj)) 
                        {
                            q.Enqueue(adj);
                        }
                        visited.Add(adj);
                    }
                }
            }

            return -1;
        }
        public List<int> searchUpByClassId(int start, int class_id, bool is_continuous = false)
        {
            Vertex<CommonJunction> vertex;
            List<int> result = new List<int>();
            vertex = network.getVertexFeature(start);

            int end_point = BFSUp<CommonJunction, CommonJunction>(network, start, (x) => false,
                (v, a) =>
                {
                    vertex = network.getVertexFeature(a);
                    if (vertex.value.ClassID == class_id)
                    {
                        result.Add(a);
                        if (!is_continuous)
                            return false;
                    }
                    return true;
                }
            );

            return result;
        }

        public List<int> searchUpArmat(int start)
        {
            Vertex<CommonJunction> vertex;
            List<int> result = new List<int>();
            vertex = network.getVertexFeature(start);

            int end_point = BFSUp<CommonJunction, CommonJunction>(network, start, (x) => false,
                (v, a) =>
                {
                    vertex = network.getVertexFeature(a);
                    if (vertex.value.ClassID == GRS_CLASS_ID)
                    {
                        throw new ArgumentException("Ошибка в данных. На пути от вершины " + start + " до источника нет задвижки");
                    }
                    if (vertex.value.ClassID == ARMAT_CLASS_ID)
                    {
                        result.Add(a);
                        return false;
                    }
                    return true;
                }
            );

            return result;
        }

        public static int BFSDownMultipleSources<TVertex, TEdge>(IDirectedNetwork<TVertex, TEdge> network, int[] sources, Func<int, bool> processVertex, Func<int, int, bool> processAdjacent) 
        {
            if (sources.Length == 0)
                return -1;

            int current = sources.First();

            HashSet<int> visited = new HashSet<int>();

            Queue<int> q = new Queue<int>();
            foreach (int v in sources)
            {
                q.Enqueue(v);
                visited.Add(v);
            }

            while (q.Count != 0)
            {
                current = q.Dequeue();

                if (processVertex(current))
                    return current;

                foreach (int adj in network.getSuccessors(current))
                {
                    if (!visited.Contains(adj))
                    {
                        if (processAdjacent(current, adj))
                        {
                            q.Enqueue(adj);
                        }
                        visited.Add(adj);
                    }
                }
            }

            return current;
        }

        public ArmatSearchResult armTask(int edgeID)
        {
            //Алгоритм не будет изменять существующую сеть, вместо этого будем изменять копию - new_net
            yEdDirectedNetwork new_net = new yEdDirectedNetwork();
            int start_vertex =  network.getEdgeFeature(edgeID).targetID;
            
            Dictionary<int, Vertex<CommonJunction>> vertexes = new Dictionary<int, Vertex<CommonJunction>>(network.Vertexes.ToDictionary(x => x.id));
            Dictionary<int, Edge<CommonJunction>> edges = new Dictionary<int, Edge<CommonJunction>>(network.Edges.ToDictionary(x => x.id));
           
            //Пункты 1, 2. Делаем обход и находим ближайшие задвижки
            List<int> arm = searchUpArmat(start_vertex);
            
            //Пункт 3. находим источники
            List<int> grs = searchUpByClassId(start_vertex, GRS_CLASS_ID);
            

            Vertex<CommonJunction> vertex;
            Edge<CommonJunction> edge;
            //Пункты 4. Закрываем задвижки, начиаем обход вниз от источников
            HashSet<int> arm_hash = new HashSet<int>(arm);
            BFSDownMultipleSources<CommonJunction, CommonJunction>(network, grs.ToArray(), 
                (x) => 
                {
                    vertex = network.getVertexFeature(x);
                    //Пункт 5. Удаляем узлы
                    vertexes.Remove(x);
                    return false;
                },
                (v, a) => 
                {
                    edge = network.getEdgeFeature(v, a);
                    //Пункт 5. Удаляем ребра
                    edges.Remove(edge.id);
                    if (arm_hash.Contains(a))
                        return false;
                    return true;
                }
            );
            
            new_net.loadFromData(vertexes, edges);
            return new ArmatSearchResult() { armat = arm, network = new_net };
        }
    }

    
    public class ArmatSearchResult 
    {
        public IDirectedNetwork<CommonJunction, CommonJunction> network;
        public List<int> armat;
    }
}
