using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomertyNetworkWorker.NetworkAnalyst
{
    public class Edge<TEdge>
    {
        public int id;
        public int sourceID;
        public int targetID;
        public TEdge value;

        public Edge(int id, int source, int target, TEdge value)
        {
            this.id = id;
            this.sourceID = source;
            this.targetID = target;
            this.value = value;
        }
    }
    public class Vertex<TVertex>
    {
        public int id;
        public TVertex value;

        public Vertex(int id, TVertex value)
        {
            this.id = id;
            this.value = value;
        }
    }


    public interface IDirectedNetwork<TVertex, TEdge>
    {
        IEnumerable<Vertex<TVertex>> Vertexes { get; }
        IEnumerable<Edge<TEdge>> Edges { get; }
        IEnumerable<int> getAdjacentEdges(int vertexID);
        IEnumerable<int> getPredecessors(int vertexID);
        IEnumerable<int> getSuccessors(int vertexID);
        Vertex<TVertex> getVertexFeature(int vertexID);
        Edge<TEdge> getEdgeFeature(int edgeID);
        Edge<TEdge> getEdgeFeature(int fromID, int toID);
        bool containsVertex(int vertexID);
        bool containsEdge(int edgeID);
        bool containsEdge(int fromID, int toID);
    }
    public class InMemoryNetowork<TVertex, TEdge> : IDirectedNetwork<TVertex, TEdge>
    {
        protected Dictionary<int, Vertex<TVertex>> _vertexes;
        protected Dictionary<int, Edge<TEdge>> _edges;
        protected Dictionary<int, HashSet<int>> _g;

        public InMemoryNetowork()
        {
            _vertexes = new Dictionary<int, Vertex<TVertex>>();
            _edges = new Dictionary<int, Edge<TEdge>>();
            _g = new Dictionary<int, HashSet<int>>();
            
        }

        public void loadFromData(Dictionary<int, Vertex<TVertex>> vertexes, Dictionary<int, Edge<TEdge>> edges)
        {
            _vertexes = vertexes;
            _edges = edges;

            foreach (Vertex<TVertex> vertex in vertexes.Values)
            {
                _g[vertex.id] = new HashSet<int>();
            }
            foreach (Edge<TEdge> edge in edges.Values)
            {
                _g[edge.sourceID].Add(edge.id);
                _g[edge.targetID].Add(edge.id);
            }
        }

        
        public IEnumerable<Vertex<TVertex>> Vertexes
        {
            get 
            {
                return _vertexes.Values;
            }
        }

        public IEnumerable<Edge<TEdge>> Edges
        {
            get 
            {
                return _edges.Values;
            }
        }

        public IEnumerable<int> getAdjacentEdges(int vertexID)
        {
            return _g[vertexID];
        }
        public IEnumerable<int> getPredecessors(int vertexID)
        {
            foreach (int edgeID in _g[vertexID])
            {
                if (_edges[edgeID].targetID == vertexID)
                {
                    yield return _edges[edgeID].sourceID;
                }
            }
        }

        public IEnumerable<int> getSuccessors(int vertexID)
        {
            foreach (int edgeID in _g[vertexID])
            {
                if (_edges[edgeID].sourceID == vertexID)
                {
                    yield return _edges[edgeID].targetID;
                }
            }
        }

        public Vertex<TVertex> getVertexFeature(int vertexID)
        {
            return _vertexes[vertexID];
        }

        public Edge<TEdge> getEdgeFeature(int edgeID)
        {
            return _edges[edgeID];
        }
        public Edge<TEdge> getEdgeFeature(int fromID, int toID)
        {
            foreach (int edgeID in _g[fromID])
            {
                if (_edges[edgeID].targetID == toID)
                    return _edges[edgeID];
            }
            throw new KeyNotFoundException("Edge " + fromID + "=>" + toID + " does not exist");
        }
        public bool containsVertex(int vertexID)
        {
            return _vertexes.ContainsKey(vertexID);
        }

        public bool containsEdge(int edgeID)
        {
            return _edges.ContainsKey(edgeID);
        }

        public bool containsEdge(int fromID, int toID)
        {
            foreach (int edgeID in _g[fromID])
            {
                Edge<TEdge> edge = _edges[edgeID];
                if (edge.sourceID == fromID && edge.targetID == toID)
                {
                    return true;
                }
            }
            return false;
        }
    }
   
    public class yEdDirectedNetwork : 
        InMemoryNetowork<CommonJunction, CommonJunction>
    {
        protected Dictionary<string, int> symbols_to_classes;
        protected Dictionary<int, int> yedid_to_id;

        public yEdDirectedNetwork()
            : base()
        {

        }
        //injecting symbols for loadFromXml
        public void setSymbols(Dictionary<string, int> symbols_to_classes)
        {
            this.symbols_to_classes = symbols_to_classes;
        }

        protected void getJunction(XElement node)
        {
            XNamespace nsy = "http://www.yworks.com/xml/graphml";

            int yed_id = int.Parse(node.Attribute("id").Value.Substring(1));
            int id = int.Parse(node.Descendants(nsy + "NodeLabel").First().Value);
            yedid_to_id.Add(yed_id, id);

            XElement xml_node = node.Descendants(nsy + "Shape").First();

            string symbol = xml_node.Attribute("type").Value;
            int classID = 0;
            if (symbols_to_classes.ContainsKey(symbol))
                classID = symbols_to_classes[symbol];
            var vertex = new Vertex<CommonJunction>(id, new CommonJunction(id, classID, "Непонятный параметр"));
            _vertexes.Add(vertex.id, vertex);
            _g[vertex.id] = new HashSet<int>();
        }

        protected void getEdgeJunction(XElement node)
        {
            XNamespace nsy = "http://www.yworks.com/xml/graphml";
            //int id = int.Parse(node.Attribute("id").Value.Substring(1));
            int id = int.Parse(node.Descendants(nsy + "EdgeLabel").First().Value);
            int sourceYedID = int.Parse(node.Attribute("source").Value.Substring(1));
            int targetYedID = int.Parse(node.Attribute("target").Value.Substring(1));

            var edge = new Edge<CommonJunction>(id, yedid_to_id[sourceYedID], yedid_to_id[targetYedID], new CommonJunction());
            _edges.Add(edge.id, edge);
            _g[edge.sourceID].Add(edge.id);
            _g[edge.targetID].Add(edge.id);
        }
        public void loadFromXml(string graphml)
        {
            if (this.symbols_to_classes == null)
            {
                this.symbols_to_classes = new Dictionary<string, int>();
            }
            yedid_to_id = new Dictionary<int, int>();
            XDocument doc = XDocument.Parse(graphml);
            XNamespace ns = "http://graphml.graphdrawing.org/xmlns";

            foreach (XElement node in doc.Descendants(ns + "node"))
            {
                getJunction(node);
            }

            foreach (XElement node in doc.Descendants(ns + "edge"))
            {
                getEdgeJunction(node);
            }
        }

    }
}
