using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Delaunay
{
    public class SaliencyMap : MonoBehaviour
    {

        private RawImage img;
        public int block = 50;

        public float a = 0.5f;
        public Texture2D source;

        private void Awake()
        {
            img = GetComponent<RawImage>();
        }


        private List<List<float>> InitBlock(Texture2D texture)
        {
            List<List<float> > blockList = new List<List<float>>(block);

            int width = texture.width / block;
            int height = texture.height / block;

            for (int x = 0; x < block; x++)
            {

                for (int y = 0; y < block; y++)
                {
                    Color c = Color.black;
                    
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            c += texture.GetPixel(i + x * width, j + y * height);
                        }
                    }

                    c /= (width * height);
                    blockList[x].Add(c.grayscale);
                }
            }


            return blockList;
        }

        public void DrawSaliencyMap(Texture2D texture)
        {
            Texture2D temp = new Texture2D(texture.width, texture.height);

            List<List<float> > ui = new List<List<float>>();
            List<List<float> > di = new List<List<float>>();

            for (int i = 0; i < temp.width; i++)
            {
                ui.Add(new List<float>());
                for (int j = 0; j < temp.height; j++)
                {
                    Color c = source.GetPixel(i, j);

                   var g = Utils.Gaussian(i - 1, j - 1, a) + Utils.Gaussian(i, j - 1, a) +
                            Utils.Gaussian(i + 1, j - 1, a) +
                            Utils.Gaussian(i - 1, j, a) + Utils.Gaussian(i, j, a) + Utils.Gaussian(i + 1, j, a) +
                            Utils.Gaussian(i - 1, j + 1, a) + Utils.Gaussian(i, j + 1, a) + Utils.Gaussian(i + 1, j + 1, a);

                    var _ui = c.grayscale * c.grayscale - 2 * c.grayscale * g + g * g;

                    ui[i].Add(_ui);
                }
            }

            for (int i = 0; i < temp.width; i++)
            {
                di.Add(new List<float>());
                for (int j = 0; j < temp.height; j++)
                {
                    Color c = source.GetPixel(i, j);

                    var g = Utils.Gaussian(i - 1, j - 1, a) + Utils.Gaussian(i, j - 1, a) +
                            Utils.Gaussian(i + 1, j - 1, a) +
                            Utils.Gaussian(i - 1, j, a) + Utils.Gaussian(i, j, a) + Utils.Gaussian(i + 1, j, a) +
                            Utils.Gaussian(i - 1, j + 1, a) + Utils.Gaussian(i, j + 1, a) + Utils.Gaussian(i + 1, j + 1, a);

                }
            }

            temp.Apply();
            img.texture = temp;
        }

        private void GetP(int x, int y)
        {
            
        }

        private Color GetC(int i)
        {
            int h = i / source.width;
            return source.GetPixel(i % source.width, h);
        }

        private float GetColorDistance(Texture2D texture2D, int x1, int x2, int y1, int y2)
        {
            Color c = texture2D.GetPixel(x1, y1) - texture2D.GetPixel(x2, y2);
            return new Vector3(c.r, c.g, c.b).normalized.magnitude;
        }

        private float GetPositionDistance(int x1, int x2, int y1, int y2)
        {
            int dx = x1 - x2;
            int dy = y1 - y2;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }


    }
}
