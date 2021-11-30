using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMPI
{
    [Serializable]
    public class Request
    {
        public int[][] FirstMatrix { get; set; }
        public int[][] SecondMatrix { get; set; }
        public int Offset { get; set; }
    }
}
