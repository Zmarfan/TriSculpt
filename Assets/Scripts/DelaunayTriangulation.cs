using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DelaunayTriangulationGenerator : MonoBehaviour
{
    //Used to be able to include points that are on the triangle perimeter
    public static float BOUNDS_EPSILON = 0.2f;

    /// <summary>
    /// Generate a delaunay triangulation from set of points that lies within certain boundary
    /// </summary>
    /// <param name="points">Set of points that will make up triangulation</param>
    /// <param name="bounds">Area the points lies within, used to make a super triangle around the points</param>
    /// <returns>A list of triangles which obeys laws of delaunay triangulation</returns>
    public static List<Triangle> GenerateDelaunayTriangulationWithPoints(List<Vector2> points, Vector2 bounds)
    {
        Triangle superTriangle = GenerateSuperTriangle(bounds);
        List<Triangle> triangulation = BowyerWatson(new List<Triangle> { superTriangle }, points);

        // Removes super triangle and triangles associated with it
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];
            if (triangle.SharesAnyVertex(superTriangle))
                triangulation.RemoveAt(i);
        }

        return triangulation;
    }

    /// <summary>
    /// Generates a randomized delaunay triangulation within certain bounds with certain amount of points
    /// </summary>
    /// <param name="pointAmount">Amount of points the triangulation will consist of</param>
    /// <param name="bounds">Bounds the triangulation will lie within</param>
    public static List<Triangle> GenerateRandomDelaunayTriangulatedGraph(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = GeneratePoints(pointAmount, bounds);

        return GenerateDelaunayTriangulationWithPoints(points, bounds);
    }

    /// <summary>
    /// Generate a random set of points within a certain boundary
    /// </summary>
    /// <param name="pointAmount">Amount of points to generate</param>
    public static List<Vector2> GeneratePoints(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointAmount; i++) // possibilty for duplicates but extremely low
        {
            float x = UnityEngine.Random.Range(0f, bounds.x);
            float y = UnityEngine.Random.Range(0f, bounds.y);
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    /// <summary>
    /// Bowyer–Watson algorithm is a method for computing the Delaunay triangulation of a finite set of points
    /// </summary>
    /// <param name="validTriangulation">A valid delaunay trianglation to insert points into</param>
    /// <param name="points">Set of points to insert into validTriangulation</param>
    /// <returns>A new delaunay triangulation with these insterted points</returns>
    public static List<Triangle> BowyerWatson(List<Triangle> validTriangulation, List<Vector2> points)
    {
        List<Triangle> triangulation = validTriangulation;

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
        Vector2 pointA = new Vector2(-BOUNDS_EPSILON, BOUNDS_EPSILON + innerBounds.y + innerBounds.x * Mathf.Tan(angleA * Mathf.Deg2Rad));
        Vector2 pointB = new Vector2(-BOUNDS_EPSILON, -BOUNDS_EPSILON);
        Vector2 pointC = new Vector2(BOUNDS_EPSILON + innerBounds.x + innerBounds.y * Mathf.Tan(angleB * Mathf.Deg2Rad), -BOUNDS_EPSILON);
        return new Triangle(pointA, pointB, pointC);
    }
}