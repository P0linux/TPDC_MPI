using MatrixMPI.Models;
using MPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MatrixMPI.Collective
{
    public class CollectiveMatrixMultiplier
    {
        public static void OneToAllMultiply(int rank)
        {
            MPI.Environment.Run(comm =>
            {
                if (comm.Rank == 0)
                {
                    if (comm.Size < 2)
                        throw new Exception("Run the programm on at least two processors");

                    var firstMatrix = MatrixHelper.CreateMatrix(rank);
                    var secondMatrix = MatrixHelper.CreateMatrix(rank);
                    var resultMatrix = MatrixHelper.CreateEmptyMatrix(rank);

                    var workersCount = comm.Size;
                    var rowsPerProcess = firstMatrix.Length / workersCount;

                    var arrangedData = new int[workersCount][][];

                    for (int i = 0; i < workersCount; i++)
                    {
                        var offset = i * rowsPerProcess;

                        var matrix = firstMatrix[offset..(offset + rowsPerProcess)];

                        arrangedData[i] = matrix;
                    }

                    var watch = new Stopwatch();
                    watch.Start();

                    var subArray = comm.Scatter(arrangedData, 0);
                    comm.Broadcast<int[][]>(ref secondMatrix, 0);

                    var resultSubArray = MatrixHelper.MultiplyMatrices(subArray, secondMatrix);

                    var receivedResult = comm.Gather<int[][]>(resultSubArray, 0);

                    for (int i = 0; i < receivedResult.Length; i++)
                    {
                        for (int k = 0; k < rowsPerProcess; k++)
                        {
                            resultMatrix[i * rowsPerProcess + k] = receivedResult[i][k];
                        }
                    }

                    watch.Stop();

                    var timeForMultiply = watch.ElapsedMilliseconds;
                    var timeForSimple = GetTimeForSimple(firstMatrix, secondMatrix);

                    var simpleMatrix = MatrixHelper.MultiplyMatrices(firstMatrix, secondMatrix);

                    Console.WriteLine($"Simple multiply : {timeForSimple}");

                    Console.WriteLine($"One-to-many multiply : {timeForMultiply}");

                    Console.WriteLine("SpeedUp: " + (float)timeForSimple / timeForMultiply);
                }
                else
                {
                    var subMatrix = comm.Scatter<int[][]>(0);
                    var secondMatrix = MatrixHelper.CreateEmptyMatrix(rank);
                    comm.Broadcast<int[][]>(ref secondMatrix, 0);

                    var resultSubMatrix = MatrixHelper.MultiplyMatrices(subMatrix, secondMatrix);
                    comm.Gather<int[][]>(resultSubMatrix, 0);
                }
            });
        }

        public static void AllToAllMultiply(int rank)
        {
            MPI.Environment.Run(comm =>
            {
                Stopwatch watch = null;
                int[][] secondMatrix, resultMatrix, firstSubArray, secondSubArray;

                if (comm.Rank == 0)
                {
                    watch = new Stopwatch();
                    watch.Start();
                }

                var rowsPerNode = rank / comm.Size;

                secondSubArray = MatrixHelper.CreateMatrix(rowsPerNode, rank);
                firstSubArray = MatrixHelper.CreateMatrix(rowsPerNode, rank);

                secondMatrix = comm.AllgatherFlattened(secondSubArray, secondSubArray.Length);

                var resultBlock = MatrixHelper.MultiplyMatrices(firstSubArray, secondMatrix);

                resultMatrix = comm.GatherFlattened(resultBlock, 0);

                if (comm.Rank == 0)
                {
                    watch.Stop();

                    var timeForMultiply = watch.ElapsedMilliseconds;

                    var timeForSimple = GetTimeForSimple(rank);

                    Console.WriteLine($"Many-to-many multiply : {timeForMultiply}");

                    Console.WriteLine($"Simple multiply : {timeForSimple}");

                    Console.WriteLine("SpeedUp: " + timeForSimple / timeForMultiply);
                }
            });

        }

        private static long GetTimeForSimple(int[][] first, int[][] second)
        {
            var watch = new Stopwatch();
            watch.Start();

            var simpleMatrix = MatrixHelper.MultiplyMatrices(first, second);

            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        private static long GetTimeForSimple(int rank)
        {
            var watch = new Stopwatch();

            var firstMatrix = MatrixHelper.CreateMatrix(rank);
            var secondMatrix = MatrixHelper.CreateMatrix(rank);

            watch.Start();

            var result = MatrixHelper.MultiplyMatrices(firstMatrix, secondMatrix);

            watch.Stop();
            return watch.ElapsedMilliseconds;
        }
    }
}
