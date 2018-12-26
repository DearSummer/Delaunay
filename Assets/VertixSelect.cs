using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Delaunay
{
    public class VertixSelect : MonoBehaviour
    {

        private RawImage img;

        private Stack<List<Vertex>> lineStack = new Stack<List<Vertex>>();

        public float a = 0.9f;
        public float threshold = 0.02f;
        private void Awake()
        {
            img = GetComponent<RawImage>();
        }

        public List<Vertex> SelectVertices(List<Vertex> edgeVertices, List<Vertex> nonedgeVertices,Texture2D texture2D,int selectCount)
        {
            List<Vertex> vertices = new List<Vertex>();

          //  edgeVertices = Select(edgeVertices, texture2D);
            foreach (var v in edgeVertices)
            {
                float p =( a * selectCount) / edgeVertices.Count;
                if(Random.value < p)
                    vertices.Add(v);
            }

            foreach (var v in nonedgeVertices)
            {
                float p = ((1 - a) * selectCount ) / nonedgeVertices.Count;
                if(Random.value < p)
                    vertices.Add(v);
            }

            Texture2D temp = new Texture2D(texture2D.width,texture2D.height);

            for (int i = 0; i < temp.width; i++)
            {
                for (int j = 0; j < temp.height; j++)
                {
                    temp.SetPixel(i, j, Color.black);
                }
            }

            foreach (var v in vertices)
            {
                temp.SetPixel(v.x,v.y,Color.white);
            }

            temp.Apply();
            img.texture = temp;
            img.SetNativeSize();

            return vertices;
        }

        public List<Vertex> Select(List<Vertex> vertices,Texture2D texture)
        {

            List<Vertex> result = new List<Vertex>();

            SpiltLine(vertices, ((texture.width + texture.height) * threshold) * ((texture.width + texture.height) * threshold));

            while (lineStack.Count != 0)
            {
                DouglasPeucher(lineStack.Pop(), 0.0002, ref result);
            }
            

            Texture2D temp = new Texture2D(texture.width,texture.height);

            for (int i = 0; i < temp.width; i++)
            {
                for (int j = 0; j < temp.height; j++)
                {
                    temp.SetPixel(i,j,Color.black);
                }
            }

            foreach (var v in result)
            {
                temp.SetPixel(v.x,v.y,Color.white);
            }

            img.texture = temp;
            
            temp.Apply();
            img.SetNativeSize();
            return result;
        }

        private double DistanceToSegmentSqrt(Vertex v, Vertex start, Vertex end)
        {
            double k = ((double)end.y - start.y) / (end.x - start.x);
            double b = end.y - k * end.x;

            double interPX = 0;
            double interPY = 0;

            if (Mathf.Abs((float)k) < float.Epsilon)
            {
                interPX = v.x;
                interPY = b;
            }
            else
            {
                var k1 = -1 / k;
                var b1 = v.y - k1 * v.x;

                interPX = (b - b1) / (k1 - k);
                interPY = k1 * interPX + b1;
            }

            return (v.x - interPX) * (v.x - interPX) + (v.y - interPY) * (v.y - interPY);



        }

        private void SpiltLine(List<Vertex> vertices, double minLength)
        {
            if (vertices.Count == 2)
            {
                lineStack.Push(vertices);
                return;
            }

            if (vertices.Count <= 1)
            {
                return;
            }


            if (GetLengthSqrt(vertices[0], vertices[vertices.Count - 1]) > minLength)
            {
                SpiltLine(vertices.Take(vertices.Count / 2 + 1).ToList(), minLength);
                SpiltLine(vertices.Skip(vertices.Count / 2 + 1).Take(vertices.Count / 2 - 1).ToList(), minLength);
            }
            else
            {
                lineStack.Push(vertices);
            }

        }

        private double GetLengthSqrt(Vertex a, Vertex b)
        {
            int x = a.x - b.x;
            int y = a.y - b.y;

            return x * x + y * y;
        }

        public void DouglasPeucher(List<Vertex> vertices,double epsilon,ref List<Vertex> result)
        {

            double dMax = 0;
            int index = 0;
            int length = vertices.Count;

            if (vertices.Count <= 2)
            {
                result.AddRange(vertices);
            }



            for (int i = 1; i < length - 1; i++)
            {
                double d = DistanceToSegmentSqrt(vertices[i], vertices[0], vertices[length - 1]);

                if (d > dMax)
                {
                    index = i;
                    dMax = d;
                }
            }

            if (dMax > epsilon * epsilon)
            {
                result.Add(vertices[0]);
                result.Add(vertices[index]);
                result.Add(vertices[length - 1]);

                DouglasPeucher(vertices.Take(index + 1).ToList(),epsilon,ref result);
                DouglasPeucher(vertices.Skip(index + 1).Take(vertices.Count - index - 1).ToList(), epsilon, ref result);
            }
        }

    }
}
