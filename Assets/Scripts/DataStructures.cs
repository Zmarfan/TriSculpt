using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Triangle
{
    public Vector2[] vertices;
    public Edge[] edges;

    public Vector2 A { get { return vertices[0]; } }
    public Vector2 B { get { return vertices[1]; } }
    public Vector2 C { get { return vertices[2]; } }

    public Edge AB { get { return edges[0]; } }
    public Edge BC { get { return edges[1]; } }
    public Edge CA { get { return edges[2]; } }

    /// <summary>
    /// The circumcenter position of the triangel
    /// </summary>
    public Vector2 Center { get; private set; }
    /// <summary>
    /// Radius of the triangel's circumencircel
    /// </summary>
    public float Radius { get; private set; }
    /// <summary>
    /// Returns true if the given point is within the triangel's circumcircel
    /// </summary>
    public bool IsPointWithinCircumference(Vector2 point)
    {
        return (point - Center).sqrMagnitude < Radius * Radius;
    }

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        this.vertices = new Vector2[] { a, b, c };
        this.edges = new Edge[] { new Edge(A, B), new Edge(B, C), new Edge(C, A) };

        // Can be optimized:)
        float alpha = ((B - C).sqrMagnitude * Vector3.Dot(A - B, A - C)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
        float beta = ((A - C).sqrMagnitude * Vector3.Dot(B - A, B - C)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
        float gamma = ((A - B).sqrMagnitude * Vector3.Dot(C - A, C - B)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);

        Center = alpha * A + beta * B + gamma * C;
        Radius = ((A - B).magnitude * (B - C).magnitude * (C - A).magnitude) / (2f * Vector3.Cross(A - B, B - C).magnitude);
    }

    /// <summary>
    /// Return true if triangles share any vertex
    /// </summary>
    public bool SharesAnyVertex(Triangle triangle)
    {
        foreach (Vector2 vertex in vertices)
        {
            for (int i = 0; i < triangle.vertices.Length; i++)
            {
                if (vertex == triangle.vertices[i])
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if triangles share any edges
    /// </summary>
    public bool SharesAnyEdge(Triangle triangle)
    {
        foreach (Edge edge in edges)
        {
            for (int i = 0; i < triangle.edges.Length; i++)
            {
                if (edge.Equals(triangle.edges[i]))
                    return true;
            }
        }
        return false;
    }
}

public class Edge
{
    public Vector2 Point1 { get; }
    public Vector2 Point2 { get; }

    public Edge(Vector2 point1, Vector2 point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (obj.GetType() != GetType())
            return false;
        Edge edge = obj as Edge;

        bool samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
        bool samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
        return samePoints || samePointsReversed;
    }

    public override int GetHashCode()
    {
        int hCode = (int)Point1.x ^ (int)Point1.y ^ (int)Point2.x ^ (int)Point2.y;
        return hCode.GetHashCode();
    }
}


/// <summary>
/// Holds amount of pixel per type of image area
/// </summary>
public class Histogram
{
    static int count = 0;
    static int testPixel = -1;

    //Key => 0-1 color value, Value => amount of that pixel
    Dictionary<float, int> _histogram = new Dictionary<float, int>();
    int _totalPixels = 0;

    /// <summary>
    /// Add a pixel to histogram
    /// </summary>
    public void AddPixel(Color pixel)
    {
        //First one of this type -> add it and initilize counter of this pixel
        if (!_histogram.ContainsKey(pixel.grayscale))
            _histogram.Add(pixel.grayscale, 1);
        else
            _histogram[pixel.grayscale]++;

        _totalPixels++;
    }

    /// <summary>
    /// Reset histogram
    /// </summary>
    public void Clear()
    {
        _histogram = new Dictionary<float, int>();
        _totalPixels = 0;
    }

    /// <summary>
    /// Calculate entropy of given pixels
    /// </summary>
    public float EntropyOfHistogram()
    {
        //Source: https://en.wiktionary.org/wiki/Shannon_entropy

        List<int> amountOfEachColor = _histogram.Values.ToList();
        float entropy = 0;

        for (int i = 0; i < amountOfEachColor.Count; i++)
            entropy += EntropyPartCalculation(amountOfEachColor[i], _totalPixels);

        if (count == testPixel)
            Debug.Log(entropy);

        count++;

        return entropy;
    }

    /// <summary>
    /// pi * log2(1/pi)
    /// </summary>
    /// <param name="amount">amount of this pixel type</param>
    /// <param name="totalAmount">total amount of pixels</param>
    float EntropyPartCalculation(int amount, int totalAmount)
    {
        return (amount / (float)totalAmount) * Mathf.Log(1 / (amount / (float)totalAmount), 2);
    }
}