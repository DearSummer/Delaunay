using UnityEngine;
using UnityEngine.UI;

namespace Delaunay
{
    public class ToGrayImage : MonoBehaviour
    {

        public Texture2D img;
        public Sobel sobel;
        private RawImage m;
        // Use this for initialization
        void Start ()
        {
            m = GetComponent<RawImage>();
            
            Texture2D temp = new Texture2D(img.width,img.height);
          

            for (int i = 0; i < img.width; i++)
            {
                for (int j = 0; j < img.height; j++)
                {
                    Color color = img.GetPixel(i,j);
                    float gray = (float) (color.r * 0.3 + color.g * 0.59 + color.b * 0.11);
                    color.r = gray;
                    color.g = gray;
                    color.b = gray;
                    temp.SetPixel(i,j,color);


                }
            }


            temp.Apply();
            m.texture = temp;
            m.SetNativeSize();
            sobel.StartSobel(temp);
        }
	
        
    }
}
