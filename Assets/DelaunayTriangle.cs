using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Delaunay
{
    public class DelaunayTriangle : MonoBehaviour
    {

        private RawImage img;

        public Texture2D source;
        public DrawTriangle drawTriangle;
        public int triangleCount = 10000;

        public VertixSelect vertixSelect;

        private bool isFinish = false;
        private List<Triangle> finalTriangles = new List<Triangle>();
        private List<Vertex> noneedgeVertices = new List<Vertex>();

        // Use this for initialization
        void Awake ()
        {
            img = GetComponent<RawImage>();
        }

        private void StartDelaunayAsync(List<Vertex> vertices)
        {
            new Thread(() =>
            {
                int length = vertices.Count;
                if (vertices.Count < 3)
                    return;

                Debug.Log("Vertices Count : " + vertices.Count);

                Triangle tri = CreateSuperTriangle(vertices);
                Debug.Log(tri.toString());

                vertices.AddRange(tri.ToVertices());

                //            List<Edge> buff = new List<Edge>();
                //            List<Triangle> triangles = new List<Triangle>{tri};

                List<int> buff = new List<int>();
                List<Circle> open = new List<Circle>();
                List<Circle> close = new List<Circle>();
                open.Add(Circumcircle(vertices, length, length + 1, length + 2));
                for (int i = 0; i < vertices.Count; i++)
                {

                    if(i % 100 == 0)
                        Debug.Log((float)i / length);
                    for (int j = open.Count - 1; j >= 0; j--)
                    {
                        int dx = vertices[i].x - open[j].point.x;
                        if (dx > 0 && dx * dx > open[j].r * open[j].r)
                        {
                            close.Add(open[j]);
                            open.RemoveAt(j);
                            continue;
                        }

                        int dy = vertices[i].y - open[j].point.y;
                        if (dx * dx + dy * dy - open[j].r * open[j].r > 0)
                            continue;

                        buff.Add(open[j].i1);
                        buff.Add(open[j].i2);
                        buff.Add(open[j].i2);
                        buff.Add(open[j].i3);
                        buff.Add(open[j].i3);
                        buff.Add(open[j].i1);
                    }

                    RemoveTheSameEdges(buff);

                    for (int d = buff.Count; d > 0;)
                    {
                        open.Add(Circumcircle(vertices, buff[--d], buff[--d], i));
                    }

                }

                close.AddRange(open);

                finalTriangles = new List<Triangle>();

                for (int i = close.Count - 1; i >= 0; i--)
                {
                    if (close[i].i1 < length && close[i].i2 < length && close[i].i3 < length)
                    {
                        Triangle t = new Triangle
                        {
                            v1 = new Vertex { x = vertices[close[i].i1].x, y = vertices[close[i].i1].y },
                            v2 = new Vertex { x = vertices[close[i].i2].x, y = vertices[close[i].i2].y },
                            v3 = new Vertex { x = vertices[close[i].i3].x, y = vertices[close[i].i3].y }
                        };

                        finalTriangles.Add(t);
                    }
                }

                Debug.Log(finalTriangles.Count);
                isFinish = true;
            }).Start();

    
        }

        private IEnumerator InitTextureAsync(Texture2D texture)
        {
            Debug.Log("inside");
            while (!isFinish)
            {
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("run");
  
            InitTextureSync(texture);
        }

        private void InitTextureSync(Texture2D texture)
        {
          Texture2D temp = new Texture2D(texture.width,texture.height);

            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    temp.SetPixel(i, j, Color.black);
                }
            }

            foreach (var t in finalTriangles)
            {
                DrawTriangle(t,temp);
            }

            temp.Apply();
            img.texture = temp;
        }

        private void DrawTriangle(Triangle tri,Texture2D texture2D)
        {

            var line1 = GetLine(tri.v1, tri.v2);
            var line2 = GetLine(tri.v1, tri.v3);
            var line3 = GetLine(tri.v2, tri.v3);

            for (int i = 0; i < line1.Count; i++)
            {
                texture2D.SetPixel(line1[i].x,line1[i].y,Color.white);
            }
            for (int i = 0; i < line2.Count; i++)
            {
                texture2D.SetPixel(line2[i].x, line2[i].y, Color.white);
            }
            for (int i = 0; i < line3.Count; i++)
            {
                texture2D.SetPixel(line3[i].x, line3[i].y, Color.white);
            }
        }

        private List<Vertex> GetLine(Vertex v1, Vertex v2)
        {

            if (v1.x > v2.x)
            {
                Vertex v = v2;
                v2 = v1;
                v1 = v;
            }

            List<Vertex> vertices = new List<Vertex>();

            float k = ((float)v1.y - v2.y) / (v1.x - v2.x);
            float b = v1.y - k * v1.x;
            for (int i = v1.x; i < v2.x; i++)
            {
                Vertex v = new Vertex {x = i, y =(int) (k * i + b)};
                vertices.Add(v);
            }

            return vertices;
        }

        public void Delaunay(Texture2D texture2D)
        {
            List<Vertex> vertices = ToVertexList(texture2D);
         //   vertices = SelectVertex(vertices);
            vertices = vertixSelect.SelectVertices(vertices,noneedgeVertices,texture2D,triangleCount);
        //    return;
           // StartDelaunay(vertices);
       
            //StartCoroutine(InitTexture(texture2D));
            int length = vertices.Count;
            if (vertices.Count < 3)
                return;

            Debug.Log("Vertices Count : " + vertices.Count);

            Triangle tri = CreateSuperTriangle(vertices);
            Debug.Log(tri.toString());

            vertices.AddRange(tri.ToVertices());
            tri.i1 = length;
            tri.i2 = length + 1;
            tri.i3 = length + 2;
            List<Triangle> triangles = new List<Triangle> {tri};
         //   triangles.Add(tri);
            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < vertices.Count; i++)
            {
                edges.Clear();

                for (int j = triangles.Count - 1;j >= 0; j--)
                {
                    if (IsInCircumcircle(vertices[i], triangles[j]))
                    {
                        edges.Add(new Edge
                        {
                            v1 = triangles[j].v1,
                            v2 = triangles[j].v2,
                            i1 = triangles[j].i1,
                            i2 = triangles[j].i2
                        });
                        edges.Add(new Edge
                        {
                            v1 = triangles[j].v1,
                            v2 = triangles[j].v3,
                            i1 = triangles[j].i1,
                            i2 = triangles[j].i3
                        });
                        edges.Add(new Edge
                        {
                            v1 = triangles[j].v2,
                            v2 = triangles[j].v3,
                            i1 = triangles[j].i2,
                            i2 = triangles[j].i3
                        });

                        triangles.RemoveAt(j);
                    }
                }

                for (int j = edges.Count - 1; j >= 0; j--)
                {
                    for(int k = j - 1; k >= 0;k--)
                    {
                        if (edges[j] == edges[k])
                        {
                            edges.RemoveAt(j);
                            edges.RemoveAt(k);
                            j--;
                            break;
                        }
                    }
                }

                for (int j = 0; j < edges.Count; j++)
                {
                    Triangle tempTriangle = new Triangle
                    {
                        v1 = edges[j].v1,
                        v2 = edges[j].v2,
                        v3 = vertices[i],
                        i1 = edges[j].i1,
                        i2 = edges[j].i2,
                        i3 = i
                    };
                    triangles.Add(tempTriangle);
                }

            }


            
            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].i1 < length && triangles[i].i2 < length && triangles[i].i3 < length)
                {
                    finalTriangles.Add(triangles[i]);
                }
            }


           Debug.Log(finalTriangles.Count);

            InitTextureSync(texture2D);
            drawTriangle.Draw(finalTriangles, source);
            img.SetNativeSize();
        }

        private bool IsInCircumcircle(Vertex v,Triangle tri)
        {
            int vx = v.x,
                vy = v.y,
                x1 = tri.v1.x,
                y1 = tri.v1.y,
                x2 = tri.v2.x,
                y2 = tri.v2.y,
                x3 = tri.v3.x,
                y3 = tri.v3.y;

            double eps = 0.000001, m1, m2, mx1, mx2, my1, my2, dx, dy, rsqr, drsqr, cx, cy, r;

            if (Mathf.Abs(y1 - y2) < eps && Mathf.Abs(y2 - y3) < eps)
            {
                return false;
            }


            if (Mathf.Abs(y2 - y1) < eps)
            {
                m2 = (double) -(x3 - x2) / (y3 - y2);
                mx2 = (double) (x2 + x3) / 2;
                my2 = (double) (y2 + y3) / 2;
                cx = (double)(x1 + x2) / 2;
                cy = m2 * (cx - mx2) + my2;
            }

            else if (Mathf.Abs(y3 - y2) < eps)
            {
                m1 = (double) -(x2 - x1) / (y2 - y1);
                mx1 = (double) (x1 + x2) / 2;
                my1 = (double) (y1 + y2) / 2;
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

            dx = x2 - cx;
            dy = y2 - cy;

            rsqr = dx * dx + dy * dy;
           // r = Mathf.Sqrt((float)rsqr);
            dx = vx - cx;
            dy = vy - cy;
            drsqr = dx * dx + dy * dy;

            return drsqr <= rsqr;
        }

        private List<Vertex> SelectVertex(List<Vertex> vertices)
        {
            int count = vertices.Count;
            int jump = count / triangleCount;

            bool needJump = triangleCount < count;

            List<Vertex> result = new List<Vertex>();
            for (int i = 0; i < vertices.Count; i++)
            {
                if(needJump && i % jump == 0)
                    result.Add(vertices[i]);
                else if(!needJump)
                    result.Add(vertices[i]);
                
            }

            return result;
        }
        private List<Vertex> ToVertexList(Texture2D texture2D)
        {
            List<Vertex> vertices = new List<Vertex>(65535);

            for (int i = 0; i < texture2D.width; i++)
            {
                for (int j = 0; j < texture2D.height; j++)
                {

                    Color color = texture2D.GetPixel(i, j);
                    Vertex v = new Vertex
                    {
                        x = i,
                        y = j
                    };
                    if (color.r != 1 || color.g != 1 || color.b != 1)
                    {
                        noneedgeVertices.Add(v);
                        continue;
                    }
//                    Vertex v = new Vertex
//                    {
//                        x = i,
//                        y = j
//                    };

                    vertices.Add(v);
                }
            }

            return vertices;

        }

        private Triangle CreateSuperTriangle(List<Vertex> vertices)
        {

            int xMax = 0, xMin = int.MaxValue, yMax = 0, yMin = int.MaxValue;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].x < xMin)
                    xMin = vertices[i].x;

                if (vertices[i].x > xMax)
                    xMax = vertices[i].x;

                if (vertices[i].y < yMin)
                    yMin = vertices[i].y;

                if (vertices[i].y > yMax)
                    yMax = vertices[i].y;
            }


            double dx = xMax - xMin;
            double dy = yMax - yMax;

            double dMax;

            if (dx > dy)
            {
                dMax = dx;
            }
            else
            {
                dMax = dy;
            }


            int xMid = (xMax + xMin) / 2;
            int yMid = (yMax + yMin) / 2;



            Triangle tri = new Triangle
            {
                v1 = new Vertex {x = (int)(xMid - 2 * dMax), y = (int) (yMid - dMax)},
                v2 = new Vertex {x = xMid, y = (int) (yMid + 2 * dMax)},
                v3 = new Vertex {x = (int) (xMid + 2 * dMax), y = (int) (yMid - dMax)}
            };

            return tri;
        }

        private Circle Circumcircle(List<Vertex> vs, int i, int j, int k)
        {
            float x1 = vs[i].x,
                x2 = vs[j].x,
                x3 = vs[k].x,
                y1 = vs[i].y,
                y2 = vs[j].y,
                y3 = vs[k].y,
                absY1Y2 = Mathf.Abs(y1 - y2),
                absY2Y3 = Mathf.Abs(y2 - y3),
                xc,
                yc,
                m1,
                m2,
                mx1,
                mx2,
                my1,
                my2;


            if (absY1Y2 < Single.Epsilon)
            {
                m2 = -((x3 - x2) / (y3 - y2));
                mx2 = (x2 + x3) / 2;
                my2 = (y2 + y3) / 2;
                xc = (x1 + x2) / 2;
                yc = m2 * (xc - mx2) + my2;
            }

            else if (absY2Y3 < Single.Epsilon)
            {
                m1 = -((x2 - x1) / (y2 - y1));
                mx1 = (x1 + x2) / 2;
                my1 = (y1 + y2) / 2;
                xc = (x3 + x2) / 2;
                yc = m1 * (xc - mx1) + my1;
            }

            else
            {
                m1 = -((x2 - x1) / (y2 - y1));
                m2 = -((x3 - x2) / (y3 - y2));
                mx1 = (x1 + x2) / 2;
                mx2 = (x2 + x3) / 2;
                my1 = (y1 + y2) / 2;
                my2 = (y2 + y3) / 2;
                xc = -(m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
                yc = (absY1Y2 > absY2Y3) ? m1 * (xc - mx1) + my1 : m2 * (xc - mx2) + my2;
            }

            var dx = x2 - xc;
            var dy = y2 - yc;

            return new Circle
            {
                point = new Vertex {x = (int)xc, y = (int)yc},
                r = Mathf.Sqrt(dx * dx + dy * dy),
                i1 = i,
                i2 = j,
                i3 = k
            };

        }

    //    private Circle Circumcircle(Triangle tri)
//        {
//            int x1 = tri.v1.x,
//                x2 = tri.v2.x,
//                x3 = tri.v3.x,
//                y1 = tri.v1.y,
//                y2 = tri.v2.y,
//                y3 = tri.v3.y;
//
//            float cx, cy;
//            cx = ((float)(x1 * x1 - x2 * x2 + y1 * y1 - y2 * y2) * (y1 - y3) -
//                  (x1 * x1 - x3 * x3 + y1 * y1 - y3 * y3) * (y1 - y2)) / (2 * (y1 - y3) * (x1 - x2) -
//                                                                          2 * (y1 - y2) * (x1 - x3));
//
//            cy = ((float)(x1 * x1 - x2 * x2 + y1 * y1 - y2 * y2) * (x1 - x3) -
//                  (x1 * x1 - x3 * x3 + y1 * y1 - y3 * y3) * (x1 - x2)) /
//                 (2 * (y1 - y2) * (x1 - x3) - 2 * (y1 - y3) * (x1 - x2));
//
//            Edge e1, e2, e3;
//            e1.v1 = tri.v1;
//            e1.v2 = tri.v2;
//            e2.v1 = tri.v1;
//            e2.v2 = tri.v3;
//            e3.v1 = tri.v2;
//            e3.v2 = tri.v3;
//
//            float a = e1.GetLength(), b = e2.GetLength(), c = e3.GetLength();
//            float s = (a + b + c) / 2;
//            float S = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
//            float R = a * b * c / 4 * S;
//
//            Circle circle = new Circle
//            {
//                point = new Vertex {x = (int) cx, y = (int) cy},
//                r = R
//            };
//
//            return circle;
//            
//
//        }

        private void RemoveTheSameEdges(List<int> edgesIndex)
        {
            for (int i = edgesIndex.Count; i > 0; )
            {
                int a = edgesIndex[--i];
                int b = edgesIndex[--i];

                for (int j = i; j > 0;)
                {
                    int n = edgesIndex[--j];
                    int m = edgesIndex[--j];

                    if ((a == m && b == n) || (a == n && b == m))
                    {
                        edgesIndex.RemoveAt(i + 1);
                        edgesIndex.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void RemoveTheSameEdges(List<Edge> edges)
        {
            for (int i = edges.Count - 1; i >= 0; i--)
            {
                for (int j = i; j > 0; j--)
                {
                    if(edges[i] == edges[j])
                        edges.RemoveAt(j);
                }
            }
        }

        private List<Triangle> AddNewTriangle(List<Edge> edges,Vertex v)
        {
            List<Triangle> triangles = new List<Triangle>();

            for (int i = 0; i < edges.Count; i++)
            {
                Triangle tri = new Triangle
                {
                    v1 = v,
                    v2 = edges[i].v1,
                    v3 = edges[i].v2
                };

                triangles.Add(tri);
            }

            return triangles;
        }

        private bool IsRelativeTri(Triangle tri, Vertex v)
        {
            return tri.v1 == v || tri.v2 == v || tri.v3 == v;
        }

    }

    

    public class Vertex
    {

        public int x, y;

        public String toString()
        {
            return "x = " + x + ", y = " + y;

        }

        public Vertex Minus(Vertex v)
        {
            return new Vertex {x = x - v.x, y = y - v.y};
        }

        public double Cross(Vertex v)
        {
            return x * v.y - y * v.x;
        }

        public static Vertex zero = new Vertex {x = 0, y = 0};

        public static bool operator == (Vertex v1, Vertex v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }

        public static bool operator !=(Vertex v1, Vertex v2)
        {
            return !(v1 == v2);
        }
    }
    public class Edge
    {
        public Vertex v1, v2;
        public int i1, i2;

        public void SetI1(int i)
        {
            i1 = i;
        }

        public float GetLength()
        {
            return Mathf.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y));
        }

        public static bool operator == (Edge e1, Edge e2)
        {
            return (e1.v2 == e2.v1 && e1.v1 == e2.v2)||(e1.v1 == e2.v1 && e1.v2 == e2.v2 );
        }

        public static bool operator !=(Edge e1, Edge e2)
        {
            return !(e1 == e2);
        }
    }
    public class Triangle
    {
        public Vertex v1, v2, v3;
        public int i1, i2, i3;

        public List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>
            {
                new Edge {v1 = v1, v2 = v2},
                new Edge {v1 = v1, v2 = v3},
                new Edge {v1 = v2, v2 = v3}
            };

            return edges;
        }

        public double CalculateTriangleArea()
        {
            Vertex ab = v2.Minus(v1);
            Vertex bc = v3.Minus(v2);
            return Mathf.Abs((float) (ab.Cross(bc) / 2));
        }

        public static bool operator ==(Triangle a, Triangle b)
        {
            return a.v1 == b.v1 && a.v2 == b.v2 && a.v3 == b.v3;
        }

        public static bool operator !=(Triangle a, Triangle b)
        {
            return !(a == b);
        }

        public List<Vertex> ToVertices()
        {
            List<Vertex> vertices = new List<Vertex> {v1,v2,v3};
            return vertices;
        }
        public String toString()
        {
            return v1.toString() + "\n" + v2.toString() + "\n" + v3.toString() + "\n";
        }
    }

    public struct Circle
    {
        public Vertex point;
        public float r;
        public int i1, i2, i3;
    }
}
