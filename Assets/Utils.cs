using UnityEngine;

namespace Delaunay
{
    public class Utils  {

        public static float Gaussian(int x, int y, float a)
        {
            return (1 / (2 * Mathf.PI * a * a)) * Mathf.Pow(2.71828182846f, -(float)(x * x + y * y) / (2 * a * a));
        }
    }
}
