using System;

namespace pico
{
    public class picoMath
    {
        /// <summary>
        /// Tests if 2 numbers are within a specified error tolerance of each other</summary>
        /// <param name="x">First number</param>
        /// <param name="y">Second number</param>
        /// <param name="tolerance">Error tolerance, absolute value, must be positive</param>
        /// <returns>True iff x and y are within the error tolerance of each other</returns>
        public static bool EqualApprox( float x, float y, float tolerance )
        {
            System.Diagnostics.Debug.Assert( tolerance >= 0 );
            float difference = Math.Abs( x - y );
            return difference <= tolerance;
        }
    }
}
