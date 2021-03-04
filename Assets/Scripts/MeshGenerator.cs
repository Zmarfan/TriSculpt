using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
    public static Mesh GenerateMeshFromTriangulation(List<Triangle> triangulation, Texture2D texture, float gradientRadiusModifier = 1)
    {
        Mesh mesh = new Mesh();
        //3 unique vertices per triangle
        Vector3[] vertices = new Vector3[triangulation.Count * 3];
        int[] triangles = new int[triangulation.Count * 3];
        Color[] colors = new Color[triangulation.Count * 3];

        for (int i = 0; i < triangulation.Count; i++)
        {
            int f1 = i * 3;
            int f2 = i * 3 + 1;
            int f3 = i * 3 + 2;

            //This order to have it face right way
            vertices[f1] = triangulation[i].A;
            vertices[f2] = triangulation[i].C;
            vertices[f3] = triangulation[i].B;

            triangles[f1] = f1;
            triangles[f2] = f2;
            triangles[f3] = f3;

            //Center of triangle
            Vector2 c = TriangleCenter(vertices[f1], vertices[f2], vertices[f3]);
            float inCircleRadius = TriangleIncircleRadius(vertices[f1], vertices[f2], vertices[f3]);

            Vector2 color1Position = GetColorPositionFromInCircle(c, inCircleRadius * gradientRadiusModifier, vertices[f1]);
            Vector2 color2Position = GetColorPositionFromInCircle(c, inCircleRadius * gradientRadiusModifier, vertices[f2]);
            Vector2 color3Position = GetColorPositionFromInCircle(c, inCircleRadius * gradientRadiusModifier, vertices[f3]);


            //Gradient (solid if gradientRadiusModifier is 0)
            colors[f1] = texture.GetPixel((int)(color1Position.x - texture.width / 2), (int)(color1Position.y - texture.height / 2));
            colors[f2] = texture.GetPixel((int)(color2Position.x - texture.width / 2), (int)(color2Position.y - texture.height / 2));
            colors[f3] = texture.GetPixel((int)(color3Position.x - texture.width / 2), (int)(color3Position.y - texture.height / 2));
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// Get point on triangle's incircle perimeter poiting toward target
    /// </summary>
    /// <param name="center">incircle center point</param>
    /// <param name="radius">Radius of incircle center point</param>
    /// <param name="target">Target point</param>
    static Vector2 GetColorPositionFromInCircle(Vector2 center, float radius, Vector2 target)
    {
        Vector2 direction = (target - center).normalized;
        return direction * radius + center;
    }

    static Vector2 TriangleCenter(Vector2 A, Vector2 B, Vector2 C)
    {
        float a = (B - C).magnitude;
        float b = (A - C).magnitude;
        float c = (A - B).magnitude;
        float perimeter = a + b + c;

        Vector2 center;
        center.x = (a * A.x + b * B.x + c * C.x) / perimeter;
        center.y = (a * A.y + b * B.y + c * C.y) / perimeter;
        return center;
    }

    static float TriangleIncircleRadius(Vector2 A, Vector2 B, Vector2 C)
    {
        //https://www.mathopenref.com/triangleincircle.html
        //https://sciencing.com/area-triangle-its-vertices-8489292.html
        float triangleArea = Mathf.Abs((A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y)) / 2);

        float a = (B - C).magnitude;
        float b = (A - C).magnitude;
        float c = (A - B).magnitude;
        float perimeter = a + b + c;

        return (2 * triangleArea) / perimeter;
    }
}
