using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GACUC
{
    class Data
    {
        public string[][] rawData = new string[7][];

        public Data()
        {
            rawData[0] = new string[] { "Blue", "Small", "False" };
            rawData[1] = new string[] { "Green", "Medium", "True" };
            rawData[2] = new string[] { "Red", "Large", "False" };
            rawData[3] = new string[] { "Red", "Small", "True" };
            rawData[4] = new string[] { "Green", "Medium", "False" };
            rawData[5] = new string[] { "Yellow", "Medium", "False" };
            rawData[6] = new string[] { "Red", "Large", "False" };
        }
    }
}
