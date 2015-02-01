using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GeomertyNetworkWorker.NetworkAnalyst
{
    public abstract class yEdFile<TVertex, TEdge>
    {
        protected Dictionary<string, int> symbols_to_classes;
        protected Dictionary<int, int> yedid_to_id;

        //injecting symbols for loadFromXml
        public void setSymbols(Dictionary<string, int> symbols_to_classes)
        {
            this.symbols_to_classes = symbols_to_classes;
        }

        protected abstract void getJunction(XElement node);
        protected abstract void getEdgeJunction(XElement node);
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
