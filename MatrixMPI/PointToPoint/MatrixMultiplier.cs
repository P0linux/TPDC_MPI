using MatrixMPI.Models;
using MPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Request = MatrixMPI.Models.Request;

namespace MatrixMPI.PointToPoint
{
    public static class MatrixMultiplier
    {
        public static void BlockingMultiply(int rank)
        {
            MPI.Environment.Run(comm =>
            {
                if (comm.Size < 2)
                    throw new Exception("Run the programm on at least two processors");

                else if (comm.Rank == 0)
                {
                    var firstMatrix = CreateMatrix(rank);
                    var secondMatrix = CreateMatrix(rank);
                    var resultMatrix = CreateEmptyMatrix(rank);

                    var workersCount = comm.Size - 1;
                    var rowsPerProcess = firstMatrix.Length / workersCount;

                    var watch = new Stopwatch();
                    watch.Start();

                    for (int i = 0; i < workersCount; i++)
                    {
                        var offset = i * rowsPerProcess;

                        var request = new Request
                        {
                            FirstMatrix = firstMatrix[offset..(offset + rowsPerProcess)],
                            SecondMatrix = secondMatrix,
                            Offset = offset
                        };

                        comm.Send(request, i + 1, 0);
                    }

                    for (int i = 0; i < workersCount; i++)
                    {
                        var responce = comm.Receive<Responce>(i + 1, 0);

                        var offset = responce.Offset;

                        for (int j = 0; j < responce.ResultMatrix.Length; j++)
                            resultMatrix[offset + j] = responce.ResultMatrix[j];
                    }

                    watch.Stop();

                    var timeForBlocking = watch.ElapsedMilliseconds;

                    watch = new Stopwatch();
                    watch.Start();

                    var simpleMatrix = MultiplyMatrices(firstMatrix, secondMatrix);

                    watch.Stop();

                    var timeForSimple = watch.ElapsedMilliseconds;
                    Console.WriteLine($"Simple multiply : {timeForSimple}");

                    Console.WriteLine($"Blocking multiply : {timeForBlocking}");

                    Console.WriteLine("SpeedUp: " + (float)timeForSimple / timeForBlocking);
                }
                else
                {
                    var request = comm.Receive<Request>(0, 0);
                    var result = MultiplyMatrices(request.FirstMatrix, request.SecondMatrix);
                    comm.Send(new Responce { ResultMatrix = result, Offset = request.Offset }, 0, 0);
                }
            });
        }

        public static void NonBlockingMultiply(int rank)
        {
            MPI.Environment.Run(comm =>
            {
                if (comm.Size < 2)
                    throw new Exception("Run the programm on at least two processors");

                else if (comm.Rank == 0)
                {
                    var firstMatrix = CreateMatrix(rank);
                    var secondMatrix = CreateMatrix(rank);
                    var resultMatrix = CreateEmptyMatrix(rank);

                    var workersCount = comm.Size - 1;
                    var rowsPerProcess = firstMatrix.Length / workersCount;

                    var requests = new RequestList();

                    var watch = new Stopwatch();
                    watch.Start();

                    for (int i = 0; i < workersCount; i++)
                    {
                        var offset = i * rowsPerProcess;

                        var request = new Request
                        {
                            FirstMatrix = firstMatrix[offset..(offset + rowsPerProcess)],
                            SecondMatrix = secondMatrix,
                            Offset = offset
                        };

                        var requestMpi = comm.ImmediateSend(request, i + 1, 0);
                        requests.Add(requestMpi);
                    }

                    requests.WaitAll();

                    var responces = new RequestList();

                    for (int i = 0; i < workersCount; i++)
                    {
                        var responce = comm.ImmediateReceive<Responce>(i + 1, 0, response =>
                        {
                            var offset = response.Offset;

                            for (int j = 0; j < response.ResultMatrix.Length; j++)
                            {
                                resultMatrix[offset + j] = response.ResultMatrix[j];
                            }
                        });

                        responces.Add(responce);
                    }

                    responces.WaitAll();

                    watch.Stop();

                    var timeForNonBlocking = watch.ElapsedMilliseconds;

                    watch = new Stopwatch();
                    watch.Start();

                    var simpleMatrix = MultiplyMatrices(firstMatrix, secondMatrix);

                    watch.Stop();

                    var timeForSimple = watch.ElapsedMilliseconds;
                    Console.WriteLine($"Simple multiply : {timeForSimple}");

                    Console.WriteLine($"Non blocking multiply : {timeForNonBlocking}");

                    Console.WriteLine("SpeedUp: " + (float)timeForSimple / timeForNonBlocking);
                }
                else
                {
                    var request = comm.ImmediateReceive<Request>(0, 0);
                    request.Wait();
                    var data = (Request)request.GetValue();
                    var result = MultiplyMatrices(data.FirstMatrix, data.SecondMatrix);
                    var responce = comm.ImmediateSend(new Responce { ResultMatrix = result, Offset = data.Offset }, 0, 0);
                    responce.Wait();
                }
            });
        }

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

        private static int[][] CreateEmptyMatrix(int rank)
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

        private static bool matricesEqual(int[][] expected, int[][] actual)
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
    }
}
