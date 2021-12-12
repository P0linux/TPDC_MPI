using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMPI
{
    public static class MatrixHelper
    {
        public static int[][] CreateMatrix(int rank)
        {
            int[][] matrix = new int[rank][];

            for (int i = 0; i < rank; i++)
            {
                matrix[i] = new int[rank];

                for (int j = 0; j < rank; j++)
                {
                    matrix[i][j] = new Random().Next(100);
                }
            }

            return matrix;
        }

        public static int[][] CreateMatrix(int rows, int columns)
        {
            int[][] matrix = new int[rows][];

            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new int[columns];

                for (int j = 0; j < columns; j++)
                {
                    matrix[i][j] = new Random().Next(100);
                }
            }
            return matrix;
        }

        public static int[][] CreateEmptyMatrix(int rows, int columns)
        {
            int[][] matrix = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new int[columns];
            }
            return matrix;
        }


        public static int[][] CreateEmptyMatrix(int rank)
        {
            int[][] matrix = new int[rank][];

            for (int i = 0; i < rank; i++)
            {
                matrix[i] = new int[rank];
            }
            return matrix;
        }

        public static int[][] MultiplyMatrices(int[][] first, int[][] second)
        {
            int[][] matrix = new int[first.Length][];

            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[i] = new int[second.Length];

                for (int j = 0; j < matrix[0].Length; j++)
                {
                    matrix[i][j] = 0;
                    for (int k = 0; k < second.Length; k++)
                    {
                        matrix[i][j] += first[i][k] * second[k][j];
                    }
                }
            }

            return matrix;
        }

        public static bool MatricesEqual(int[][] expected, int[][] actual)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                for (int j = 0; j < expected[0].Length; j++)
                {
                    if (expected[i][j] != actual[i][j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void PrintMatrix(int[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    Console.Write(matrix[i][j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
