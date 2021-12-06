using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GACUC
{
    class Show
    {
        public void Display()
        {
            Data data = new Data();
            Console.WriteLine("Raw unclustered data:\n");
            Console.WriteLine(" Color Size Heavy");
            Console.WriteLine("-----------------------------");
            ShowData(data.rawData);
            int numClusters = 2;
            Console.WriteLine("\nSetting numClusters to " + numClusters);
            int numRestarts = 4;
            Console.WriteLine("Setting numRestarts to " + numRestarts);
            Console.WriteLine("\nStarting clustering using greedy category utility");

            Clusterer cc = new Clusterer(numClusters, data.rawData); // restart version
            double cu;
            int[] clustering = cc.Cluster(numRestarts, out cu);
            Console.WriteLine("Clustering complete\n");
            Console.WriteLine("Final clustering in internal form:");
            ShowVector(clustering, true);
            Console.WriteLine("Final CU value = " + cu.ToString("F4"));
            Console.WriteLine("\nRaw data grouped by cluster:\n");
            ShowClustering(numClusters, clustering, data.rawData);
            Console.WriteLine("\nEnd categorical data clustering demo\n");
        }


        static void ShowData(string[][] matrix) // for tuples
        {
            for (int i = 0; i < matrix.Length; ++i)
            {
                Console.Write("[" + i + "] ");
                for (int j = 0; j < matrix[i].Length; ++j)
                    Console.Write(matrix[i][j].ToString().PadRight(8) + " ");
                Console.WriteLine("");
            }
        }
        public static void ShowVector(int[] vector, bool newLine) // for clustering
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i] + " ");
            Console.WriteLine("");
            if (newLine == true)
                Console.WriteLine("");
        }
        static void ShowClustering(int numClusters, int[] clustering, string[][] rawData)
        {
            Console.WriteLine("-----------------------------");
            for (int k = 0; k < numClusters; ++k) // display by cluster
            {
                for (int i = 0; i < rawData.Length; ++i) // each tuple
                {
                    if (clustering[i] == k) // curr tuple i belongs to curr cluster k
                    {
                        Console.Write(i.ToString().PadLeft(2) + " ");
                        for (int j = 0; j < rawData[i].Length; ++j)
                        {
                            Console.Write(rawData[i][j].ToString().PadRight(8) + " ");
                        }
                        Console.WriteLine("");
                    }
                }
                Console.WriteLine("-----------------------------");
            }
        }
    }
}
