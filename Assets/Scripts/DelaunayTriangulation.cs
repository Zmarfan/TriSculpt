using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DelaunayTriangulationGenerator : MonoBehaviour
{
    public static List<Triangle> GenerateDelaunayTriangulationWithPoints(List<Vector2> points, Vector2 bounds)
    {
        Triangle superTriangle = GenerateSuperTriangle(bounds);
        List<Triangle> triangulation = BowyerWatson(new List<Triangle> { superTriangle }, points, bounds);

        // Removes super triangle
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];
            if (triangle.SharesAnyVertex(superTriangle))
                triangulation.RemoveAt(i);
        }

        return triangulation;
    }

    public static List<Triangle> GenerateRandomDelaunayTriangulatedGraph(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = GeneratePoints(pointAmount, bounds);

        return GenerateDelaunayTriangulationWithPoints(points, bounds);
    }

    public static List<Vector2> GeneratePoints(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointAmount; i++) // possibilty for duplicates
        {
            float x = UnityEngine.Random.Range(0f, bounds.x);
            float y = UnityEngine.Random.Range(0f, bounds.y);
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    public static List<Triangle> BowyerWatson(List<Triangle> mesh, List<Vector2> points, Vector2 bounds)
    {
        List<Triangle> triangulation = mesh;

        // Add all points sequentially to the triangulation
        foreach (Vector2 point in points)
        {
            // Finds all triangles no longer valid after point insertion
            HashSet<Triangle> badTriangles = new HashSet<Triangle>();
            foreach (Triangle triangle in triangulation)
            {
                if (triangle.IsPointWithinCircumference(point))
                    badTriangles.Add(triangle);
            }

            // Handle bad triangles
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.A, triangle.B));
                edges.Add(new Edge(triangle.B, triangle.C));
                edges.Add(new Edge(triangle.C, triangle.A));
            }

            foreach (Triangle triangle in badTriangles)
                triangulation.Remove(triangle);

            // all edges around triangulation
            HashSet<Edge> boundry = new HashSet<Edge>();
            // Gets edges without duplicates
            boundry = new HashSet<Edge>(edges.GroupBy(x => x).Where(g => g.Count() == 1).Select(x => x.Key));

            // Creates new triangles connecting to added point
            foreach (Edge edge in boundry)
            {
                // forms a triangle from edge to point
                Triangle newTriangle = new Triangle(edge.Point1, edge.Point2, point);
                triangulation.Add(newTriangle);
            }
        }

        return triangulation;
    }

    public static void RemoveTrianglesOutsideOfBounds(Vector2 bounds, ref List<Triangle> triangulation)
    {
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];

            if (triangle.Center.x > bounds.x ||
                triangle.Center.x < 0 ||
                triangle.Center.y > bounds.y ||
                triangle.Center.y < 0)
            {
                triangulation.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Generate a right triangle which includes innerbounds fully
    /// </summary>
    /// <param name="innerBounds">Rectangle that must be included</param>
    /// <param name="angle">Aesthetic, leave blank if unsure (0 - 90)</param>
    public static Triangle GenerateSuperTriangle(Vector2 innerBounds, float angle = 45)
    {
        angle = Mathf.Clamp(angle, 0f, 90f);

        float angleA = angle;
        float angleB = 90 - angle;
        Vector2 pointA = new Vector2(0, innerBounds.y + innerBounds.x * Mathf.Tan(angleA * Mathf.Deg2Rad));
        Vector2 pointB = Vector2.zero;
        Vector2 pointC = new Vector2(innerBounds.x + innerBounds.y * Mathf.Tan(angleB * Mathf.Deg2Rad), 0);
        return new Triangle(pointA, pointB, pointC);
    }
}