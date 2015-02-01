﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeomertyNetworkWorker.NetworkAnalyst
{
    

    /// <summary>
    /// Read-only network interface
    /// </summary>
    /// <typeparam name="TVertex">Тип вершин, например CommonJunction</typeparam>
    /// <typeparam name="TEdge">Тип ребер, например int</typeparam>
    public interface IGeometricNetwork<TVertex, TEdge>
    {
        IEnumerable<Vertex<TVertex>> Vertexes { get; }
        IEnumerable<Edge<TEdge>> Edges { get; }

        IEnumerable<int> getAdjacentVertex(int vertexID);
        Vertex<TVertex> getVertex(int vertexID);
        int getEdge(int fromID, int toID);
        bool tryGetEdge(int fromID, int toID, out int outEdge);
        bool containsVertex(int vertexID);
        bool tryGetEdgeFeature(int edgeID, out Edge<TEdge> edge);
    }

    
    public abstract class NetworkTracer<TVertex, TEdge>
    {
        public IGeometricNetwork<TVertex, TEdge> network;
        private HashSet<int> visited;

        protected abstract void processVertex(Vertex<TVertex> vertex, int vertexID);
        protected abstract void processEdge(Edge<TEdge> edge, int edgeID);
        protected abstract bool adjacentContract(int current, int adjacent);
        //breadth-first search или поиск в ширину
        public void BFS(int startID, Action<Vertex<TVertex>, int> processV, Action<Edge<TEdge>, int> processE, Func<int,int,bool> adjContract)
        {
            if (visited == null)
                visited = new HashSet<int>();

            Queue<int> q = new Queue<int>();
            visited.Clear();
            q.Enqueue(startID);
            visited.Add(startID);
            int current;
            Edge<TEdge> currentEdge;
            int currentEdgeID;

            while (q.Count != 0)
            {
                current = q.Dequeue();

                int[] adjacent = network.getAdjacentVertex(current).ToArray();
                
                foreach(int v in adjacent)
                {
                    if (!visited.Contains(v) && adjContract(current, v))
                    {
                        processV(network.getVertex(v), v);
                        if(network.tryGetEdge(current, v, out currentEdgeID) && network.tryGetEdgeFeature(currentEdgeID, out currentEdge)) 
                        {
                            processE(currentEdge, currentEdgeID);
                        }
                        
                        q.Enqueue(v);
                        visited.Add(v);
                    }
                }

            }
        }

        public void BFS(int startID)
        {
            BFS(startID, processVertex, processEdge, adjacentContract);
        }
    }

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
        public int BFSDown(int start, Func<int, bool> processVertex, Func<int,int,bool> processAdjacent) 
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

            return current;
        }
        public List<int> searchDownByClassId(int start, int class_id, bool is_continuous = false)
        {
            Vertex<CommonJunction> vertex;
            List<int> result = new List<int>();
            vertex = network.getVertexFeature(start);

            int end_point = BFSDown(start, (x) => false,
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

        public List<int> searchDownArmat(int start)
        {
            Vertex<CommonJunction> vertex;
            List<int> result = new List<int>();
            vertex = network.getVertexFeature(start);

            int end_point = BFSDown(start, (x) => false,
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

        public int BFSUpMultipleSources(int[] sources, Func<int, bool> processVertex, Func<int, int, bool> processAdjacent) 
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
            //Создаем 
            yEdDirectedNetwork new_net = new yEdDirectedNetwork();

            int start_vertex =  network.getEdgeFeature(edgeID).targetID;
            
            Dictionary<int, Vertex<CommonJunction>> vertexes = new Dictionary<int, Vertex<CommonJunction>>(network.Vertexes.ToDictionary(x => x.id));
            Dictionary<int, Edge<CommonJunction>> edges = new Dictionary<int, Edge<CommonJunction>>(network.Edges.ToDictionary(x => x.id));
           
            //находим задвижки
            List<int> arm = searchDownArmat(start_vertex);
            //находим грс
            List<int> grs = searchDownByClassId(start_vertex, GRS_CLASS_ID);
            HashSet<int> arm_hash = new HashSet<int>(arm);

            Vertex<CommonJunction> vertex;
            Edge<CommonJunction> edge;
            //находим участок сети от грс до задвижек
            BFSUpMultipleSources(grs.ToArray(), 
                (x) => 
                {
                    vertex = network.getVertexFeature(x);
                    vertexes.Remove(x);
                    return false;
                },
                (v, a) => 
                {
                    edge = network.getEdgeFeature(v, a);
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