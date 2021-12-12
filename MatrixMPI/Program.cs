// See https://aka.ms/new-console-template for more information
using MatrixMPI.Collective;
using MatrixMPI.PointToPoint;

var rank = 1200;

//CollectiveMatrixMultiplier.OneToAllMultiply(rank);
CollectiveMatrixMultiplier.AllToAllMultiply(args);


