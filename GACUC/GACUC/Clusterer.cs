using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GACUC
{
    class Clusterer
    {
        private int numClusters; // number of clusters
        private int[] clustering; // index = a tuple, value = cluster ID
        private int[][] dataAsInts; // ex: red = 0, blue = 1, green = 2
        private int[][][] valueCounts; // scratch to compute CU [att][val][count](sum)
        private int[] clusterCounts; // number tuples assigned to each cluster (sum)
        private Random rnd; // for several randomizations

        public Clusterer(int numClusters, string[][] rawData)
        {
            this.numClusters = numClusters;
            MakeDataMatrix(rawData);
            Allocate();
        }
        public int[] Cluster(int numRestarts, out double catUtility)
        {
            int rows = dataAsInts.Length;
            double currCU, bestCU = 0.0;
            int[] bestClustering = new int[rows];
            for (int i = 0; i < numRestarts; i++)
            {
                int seed = i;
                int[] currClustering = ClusterOnce(seed, out currCU);
                if (currCU > bestCU)
                {
                    bestCU = currCU;
                    Array.Copy(currClustering, bestClustering, rows);
                }
            }
            catUtility = bestCU;
            return bestClustering;
        }

        private int[] ClusterOnce(int seed, out double currCU)
        {
            this.rnd = new Random(seed);
            Initialize();
            int numTrials = dataAsInts.Length;
            int[] goodIndexes = GetGoodIndices(numTrials);
            for (int i = 0; i < numClusters; i++)
            {
                Assign(goodIndexes[i], i);
            }
            int rows = dataAsInts.Length;
            int[] rndSequence = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                rndSequence[i] = i;
            }
            Shuffle(rndSequence);
            for (int i = 0; i < rows; i++)
            {
                int dtx = rndSequence[i];
                if (clustering[dtx] != -1)
                {
                    continue;
                }
                double[] candidateCU = new double[numClusters];
                for (int j = 0; j < numClusters; j++)
                {
                    Assign(dtx, j);
                    candidateCU[j] = CategoryUtility();
                    Unassign(dtx, j);
                }
                int bestK = MaxIndex(candidateCU);
                Assign(dtx, bestK);
            }
            currCU = CategoryUtility();
            int[] result = new int[rows];
            Array.Copy(this.clustering, result, rows);
            return result;
        }

        private void MakeDataMatrix(string[][] rawData)
        {
            int rows = rawData.Length;
            int cols = rawData[0].Length;

            this.dataAsInts = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                dataAsInts[i] = new int[cols];
            }
            for (int i = 0; i < cols; i++)
            {
                int index = 0;
                Dictionary<string, int> dict = new Dictionary<string, int>();
                for (int j = 0; j < rows; j++)
                {
                    string s = rawData[j][i];
                    if (dict.ContainsKey(s) == false)
                    {
                        dict.Add(s, index++);
                    }
                    int v = dict[s];
                    this.dataAsInts[j][i] = v;
                }
            }
        }

        private void Allocate()
        {
            int rows = dataAsInts.Length;
            int cols = dataAsInts[0].Length;

            this.clustering = new int[rows];
            this.clusterCounts = new int[numClusters + 1];
            this.valueCounts = new int[cols][][];

            for (int i = 0; i < cols; i++)
            {
                int maxVal = 0;
                for (int j = 0; j < rows; j++)
                {
                    if (dataAsInts[j][i] > maxVal)
                    {
                        maxVal = dataAsInts[j][i];
                    }
                }
                this.valueCounts[i] = new int[maxVal + 1][];
            }
            for (int i = 0; i < this.valueCounts.Length; i++)
            {
                for (int j = 0; j < this.valueCounts[i].Length; j++)
                {
                    this.valueCounts[i][j] = new int[numClusters + 1];
                }
            }
        }
        private void Initialize()
        {
            for (int i = 0; i < clustering.Length; i++)
            {
                clustering[i] = -1;
            }
            for (int i = 0; i < clusterCounts.Length; i++)
            {
                clusterCounts[i] = 0;
            }
            for (int i = 0; i < valueCounts.Length; i++)
            {
                for (int j = 0; j < valueCounts[i].Length; j++)
                {
                    for (int k = 0; k < valueCounts[i][j].Length; k++)
                    {
                        valueCounts[i][j][k] = 0;
                    }
                }
            }
        }
        private double CategoryUtility()
        {
            int numOfAssignedData = clusterCounts[clusterCounts.Length - 1];
            double[] clusterProbs = new double[this.numClusters];
            for (int i = 0; i < numClusters; i++)
            {
                clusterProbs[i] = (double)clusterCounts[i] / numOfAssignedData;
            }
            double unconditional = 0.0;
            for (int i = 0; i < valueCounts.Length; i++)
            {
                for (int j = 0; j < valueCounts[i].Length; j++)
                {
                    int sum = valueCounts[i][j][numClusters];
                    double p = (double)sum / numOfAssignedData;
                    unconditional += (p * p);
                }
            }

            double[] conditionals = new double[numClusters];
            for (int i = 0; i < numClusters; i++)
            {
                for (int j = 0; j < valueCounts.Length; j++)
                {
                    for (int k = 0; k < valueCounts[j].Length; k++)
                    {
                        double p = (double)valueCounts[j][k][i] / clusterCounts[i];
                        conditionals[i] += (p * p);
                    }
                }
            }

            double summation = 0.0;
            for (int i = 0; i < numClusters; i++)
            {
                summation += clusterProbs[i] * (conditionals[i] - unconditional);
            }
            return summation / numClusters;
        }

        private static int MaxIndex(double[] CUs)
            =>
                Array.IndexOf(CUs, CUs.Max());

        private void Shuffle(int[] indices)
        {
            rnd = new Random();
            for (int i = 0; i < indices.Length; i++)
            {
                int randIndex = rnd.Next(i, indices.Length);
                int temp = indices[i];
                indices[i] = indices[randIndex];
                indices[randIndex] = temp;
            }
        }
        private void Assign(int dataIndex, int clusterId)
        {
            clustering[dataIndex] = clusterId;

            for (int i = 0; i < valueCounts.Length; i++)
            {
                int v = dataAsInts[dataIndex][i];
                ++valueCounts[i][v][clusterId];
                ++valueCounts[i][v][numClusters];
            }
            ++clusterCounts[clusterId];
            ++clusterCounts[numClusters];
        }
        private void Unassign(int dataIndex, int clusterId)
        {
            clustering[dataIndex] = -1;

            for (int i = 0; i < valueCounts.Length; i++)
            {
                int v = dataAsInts[dataIndex][i];
                --valueCounts[i][v][clusterId];
                --valueCounts[i][v][numClusters];
            }
            --clusterCounts[clusterId];
            --clusterCounts[numClusters];
        }
        private int[] GetGoodIndices(int numTrials)
        {
            int rows = dataAsInts.Length;
            int cols = dataAsInts[0].Length;
            int[] result = new int[numClusters];

            int largestDiff = -1;
            for (int i = 0; i < numTrials; i++)
            {
                int[] candidates = Reservoir(numClusters, rows);
                int numDifferences = 0;

                for (int j = 0; j < candidates.Length - 1; j++)
                {
                    for (int k = 0; k < candidates.Length; k++)
                    {
                        int rowA = candidates[j];
                        int rowB = candidates[k];

                        for (int n = 0; n < cols; n++)
                        {
                            if (dataAsInts[rowA][n] != dataAsInts[rowB][n])
                            {
                                ++numDifferences;
                            }
                        }
                    }
                }

                if (numDifferences > largestDiff)
                {
                    largestDiff = numDifferences;
                    Array.Copy(candidates, result, numClusters);
                }
            }
            return result;
        }

        private int[] Reservoir(int numClusters, int rows)
        {
            rnd = new Random();
            int[] result = new int[numClusters];
            for (int i = 0; i < numClusters; i++)
            {
                result[i] = i;
            }
            for (int i = 0; i < rows; i++)
            {
                int j = rnd.Next(0, i + 1);
                if (j < numClusters)
                {
                    result[j] = i;
                }
            }
            return result;
        }
    }
}
