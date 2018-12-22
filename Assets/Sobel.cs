using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Delaunay
{
    public class Sobel : MonoBehaviour
    {
        public DelaunayTriangle delaunayTriangle;
        
        private RawImage r;

        private Texture2D img;

        public float threshold = 1f;
        public float a = 0.1f;
        // Use this for initialization
        void Awake ()
        {
            r = GetComponent<RawImage>();
        }


        
        public void StartSobel(Texture2D grayImg)
        {
            img = grayImg;
            Texture2D temp = new Texture2D(grayImg.width, grayImg.height);

//            for (int i = 1; i < grayImg.width - 1; i++)
//            {
//                for (int j = 1; j < grayImg.height - 1; j++)
//                {
//
//                    /*
//                     * we have two factor
//                     * [ -1 0 1]   [1   2   1]
//                     * [ -2 0 2]   [0   0   0]
//                     * [ -1 0 1]   [-1 -2  -1]
//                     *
//                     *[-3  0  3]   [ 3   10   3]
//                     *[-10 0 10]   [ 0    0   0]
//                     *[-3  0  3]   [-3   10  -3]
//                     *
//                     */
//
//                    var g = GetGray(i - 1, j - 1) * Utils.Gaussian(i - 1, j - 1, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i, j - 1, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i + 1, j - 1, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i - 1, j, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i, j, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i + 1, j, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i - 1, j + 1, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i, j + 1, a) +
//                            GetGray(i - 1, j - 1) * Utils.Gaussian(i + 1, j + 1, a);
//
////                    var gy = 3 * Gaussian(i - 1, j - 1, 0.1f) + 10 * Gaussian(i, j - 1, 0.1f) +
////                             3 * Gaussian(i + 1, j - 1, 0.1f) +
////                             0 * Gaussian(i - 1, j, 0.1f) + 0 * Gaussian(i, j, 0.1f) + 0 * Gaussian(i + 1, j, 0.1f) +
////                             -3 * Gaussian(i - 1, j + 1, 0.1f) + -10 * Gaussian(i, j + 1, 0.1f) +
////                             -3 * Gaussian(i + 1, j + 1, 0.1f);
//
//              //      var g = Mathf.Sqrt(gx * gx + gy * gy);
//                    temp.SetPixel(i, j, new Color(g, g, g));
//                }
//            }

            for (int i = 1; i < grayImg.width - 1; i++)
            {
                for (int j = 1; j < grayImg.height - 1; j++)
                {

                    /*
                     * we have two factor
                     * [ -1 0 1]   [1   2   1]
                     * [ -2 0 2]   [0   0   0]
                     * [ -1 0 1]   [-1 -2  -1]
                     *
                     *[-3  0  3]   [ 3   10   3]
                     *[-10 0 10]   [ 0    0   0]
                     *[-3  0  3]   [-3   10  -3]
                     *
                     */
                    var gx = -3 * GetGray(i - 1, j - 1) + 0 * GetGray(i, j - 1) + 3 * GetGray(i + 1, j - 1) +
                               -10 * GetGray(i - 1, j) + 0 * GetGray(i, j) + 10 * GetGray(i + 1, j) +
                               -3 * GetGray(i - 1, j + 1) + 0 * GetGray(i, j + 1) + 3 * GetGray(i + 1, j + 1);

                    var gy = 3 * GetGray(i - 1, j - 1) + 10 * GetGray(i, j - 1) + 3 * GetGray(i + 1, j - 1) +
                               0 * GetGray(i - 1, j) + 0 * GetGray(i, j) + 0 * GetGray(i + 1, j) +
                               -3 * GetGray(i - 1, j + 1) + -10 * GetGray(i, j + 1) + -3 * GetGray(i + 1, j + 1);

                  
                    var g = Mathf.Sqrt(gx * gx + gy * gy);
                    //g > 0.82f ? new Color(1, 1, 1) : new Color(0, 0, 0)
                    temp.SetPixel(i, j, g > threshold ? new Color(1, 1, 1) : new Color(0, 0, 0));
     

                }
            }
            temp.Apply();
            r.texture = temp;
            r.SetNativeSize();

            delaunayTriangle.Delaunay(temp);
        }

        private float GetGray(int x, int y)
        {
            return img.GetPixel(x, y).g;
        }
	

        
    }
}
