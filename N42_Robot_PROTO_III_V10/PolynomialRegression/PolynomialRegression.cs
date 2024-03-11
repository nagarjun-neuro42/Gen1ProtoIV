using MathNet.Numerics.LinearAlgebra;
using System;


namespace n42_Robot_PROTO_III
{
    public class PolynomialRegression
    {
        private Vector<double> coefficients;

        public void Fit(double[] x, double[] y, int degree)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("Input arrays x and y must have the same length.");
            }

            var vandermonde = Matrix<double>.Build.Dense(x.Length, degree + 1);

            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j <= degree; j++)
                {
                    vandermonde[i, j] = Math.Pow(x[i], j);
                }
            }

            var yVector = Vector<double>.Build.Dense(y);

            coefficients = vandermonde.QR().Solve(yVector);
        }

        public double Compute(double x)
        {
            double result = 0;

            for (int i = 0; i < coefficients.Count; i++)
            {
                result += coefficients[i] * Math.Pow(x, i);
            }

            return result;
        }
    }
}

