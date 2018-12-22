using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Delaunay
{
    public class DrawTriangle : MonoBehaviour
    {

        private RawImage img;
        private Texture2D source;
        private List<Vertex> vertices = new List<Vertex>(64);

        private void Awake()
        {
            img = GetComponent<RawImage>();
        }


        public void Draw(List<Triangle> triangles, Texture2D source)
        {
            Texture2D temp = new Texture2D(source.width, source.height);
            this.source = source;
            for (int i = 0; i < temp.width; i++)
            {
                for (int j = 0; j < temp.height; j++)
                {
                    temp.SetPixel(i, j, source.GetPixel(i, j));
                }
            }

            foreach (var t in triangles)
            {
                var xMin = t.v1.x < t.v2.x ? t.v1.x : t.v2.x;
                xMin = xMin < t.v3.x ? xMin: t.v3.x;

                var yMin = t.v1.y < t.v2.y ? t.v1.y : t.v2.y;
                yMin = yMin < t.v3.y ? yMin : t.v3.y;

                var xMax = t.v1.x > t.v2.x ? t.v1.x : t.v2.x;
                xMax = xMax > t.v3.x ? xMax : t.v3.x;

                var yMax = t.v1.y > t.v2.y ? t.v1.y : t.v2.y;
                yMax = yMax > t.v3.y ? yMax : t.v3.y;

                DrawATriangle(temp, t, xMin, xMax, yMin, yMax);
//                for (int i = xMin; i <= xMax; i++)
//                {
//                    for (int j = yMin; j <= yMax; j++)
//                    {
//                        DrawTriangleColor(t, source, temp, i, j);
//                    }
//                }
            }

            temp.Apply();
            img.texture = temp;
            img.SetNativeSize();

            SaveImg(temp);
        }

        private void SaveImg(Texture2D texture)
        {
            byte[] bytes = texture.EncodeToJPG();
            File.WriteAllBytes(Application.dataPath + "/_" + Time.time + ".jpg", bytes);
        }

        private void DrawATriangle(Texture2D texture2D, Triangle tri ,int xMin,int xMax,int yMin,int yMax)
        {
            vertices.Clear();
            Color c = new Color(0, 0, 0);
            int colorCount = 0;
            for (int i = xMin; i <= xMax; i++)
            {
                for (int j = yMin; j <= yMax; j++)
                {
                    Vertex v = new Vertex {x = i, y = j};
                    if(IsInTriangle(tri,v))
                    {
                        if (Random.value < 0.6f)
                        {
                            c += source.GetPixel(i, j);
                            colorCount++;
                        }

                        vertices.Add(v);
                        //                       texture2D.SetPixel(i, j, GetColor(tri, source));
                    }
                }
            }

            c =  c / colorCount;
            foreach (var v in vertices)
            {
                texture2D.SetPixel(v.x, v.y, c);
            }
        }

        private bool IsInTriangle(Triangle tri, Vertex v)
        {
            var area_ABC = tri.CalculateTriangleArea();

            var area_PAB = new Triangle {v1 = v, v2 = tri.v1, v3 = tri.v2}.CalculateTriangleArea();
            var area_PAC = new Triangle {v1 = v, v2 = tri.v1, v3 = tri.v3}.CalculateTriangleArea();
            var area_PBC = new Triangle {v1 = v, v2 = tri.v2, v3 = tri.v3}.CalculateTriangleArea();

            return Mathf.Abs((float) (area_ABC - area_PBC - area_PAB - area_PAC)) < float.Epsilon;
        }

        private bool DrawTriangleColor(Triangle tri, Texture2D source, Texture2D target, int x, int y)
        {
            if (!IsInTriangle(tri, new Vertex {x = x, y = y})) return false;

            target.SetPixel(x, y, GetColor(tri, source));
            return true;

        }


        private Color GetColor(Triangle tri,Texture2D source)
        {
            int 
                x1 = tri.v1.x,
                y1 = tri.v1.y,
                x2 = tri.v2.x,
                y2 = tri.v2.y,
                x3 = tri.v3.x,
                y3 = tri.v3.y;

            double eps = 0.000001, m1, m2, mx1, mx2, my1, my2,cx, cy;

            if (Mathf.Abs(y2 - y1) < eps)
            {
                m2 = (double)-(x3 - x2) / (y3 - y2);
                mx2 = (double)(x2 + x3) / 2;
                my2 = (double)(y2 + y3) / 2;
                cx = (double)(x1 + x2) / 2;
                cy = m2 * (cx - mx2) + my2;
            }

            else if (Mathf.Abs(y3 - y2) < eps)
            {
                m1 = (double)-(x2 - x1) / (y2 - y1);
                mx1 = (double)(x1 + x2) / 2;
                my1 = (double)(y1 + y2) / 2;
                cx = (double)(x3 + x2) / 2;
                cy = m1 * (cx - mx1) + my1;
            }

            else
            {
                m1 = (double)-(x2 - x1) / (y2 - y1);
                m2 = (double)-(x3 - x2) / (y3 - y2);
                mx1 = (double)(x1 + x2) / 2;
                mx2 = (double)(x2 + x3) / 2;
                my1 = (double)(y1 + y2) / 2;
                my2 = (double)(y2 + y3) / 2;
                cx = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                cy = m1 * (cx - mx1) + my1;
            }

            var c1 = source.GetPixel(tri.v1.x, tri.v1.y);
            var c2 = source.GetPixel(tri.v2.x, tri.v2.y);
            var c3 = source.GetPixel(tri.v3.x, tri.v3.y);
            var c4 = source.GetPixel((int) cx, (int) cy);

            
            return (c1 + c2 + c3 + c4) / 4;
        }



    }
}
