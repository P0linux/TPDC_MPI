// See https://aka.ms/new-console-template for more information
using MatrixMPI;

var rank = 1000;

MatrixMultiplier.NonBlockingMultiply(rank);
MatrixMultiplier.BlockingMultiply(rank);



