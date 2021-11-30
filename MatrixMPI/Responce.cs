using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMPI
{
    [Serializable]
    public class Responce
    {
        public int[][] ResultMatrix { get; set; }
        public int Offset { get; set; }
    }
}
