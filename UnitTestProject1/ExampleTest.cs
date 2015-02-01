using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeomertyNetworkWorker.NetworkAnalyst;
using System.Linq;

namespace UnitTestProject1
{
    public abstract class ExampleTest
    {
        const int GRS_CLASS_ID = 1;
        const int ABON_CLASS_ID = 2;
        const int ARMAT_CLASS_ID = 3;

        protected Dictionary<string, int> symbols_to_classes = new Dictionary<string, int>()
        {
            { "triangle", ARMAT_CLASS_ID },
            { "hexagon", GRS_CLASS_ID }
        };

        protected string getExamplePath(string name)
        {
            string path = getFixturePath();
            return path + "/" + name + ".graphml";
        }
        protected string getExample(string name)
        {
            string path = getFixturePath();
            string filename = path + "/" + name + ".graphml";
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }
            string example;
            using (StreamReader sr = new StreamReader(filename))
            {
                example = sr.ReadToEnd();
            }
            return example;
        }
        protected string getFixturePath()
        {
            return Environment.CurrentDirectory + "/../../fixtures/" + getRelativePath();
        }

        protected abstract string getRelativePath();

        protected bool compareCollections<T>(IEnumerable<T> arr1, IEnumerable<T> arr2) where T : IComparable<T>
        {
            foreach (T item in arr1)
            {
                if (!arr2.Contains(item))
                    return false;
            }
            if (arr1.Count() != arr2.Count())
                return false;
            return true;
        }
        protected bool compareNetworks<TVertex, TEdge>(IDirectedNetwork<TVertex, TEdge> n1, IDirectedNetwork<TVertex, TEdge> n2)
        {
            bool equal_vertexes = compareCollections(n1.Vertexes.Select(x => x.id), n2.Vertexes.Select(x => x.id));
            bool equal_edges = compareCollections(n1.Edges.Select(x => x.id), n2.Edges.Select(x => x.id));

            if (equal_edges && equal_vertexes)
            {
                return true;
            }
            return false;
        }
    }
}
