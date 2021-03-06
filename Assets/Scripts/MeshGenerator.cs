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

            Vector2 color1Position = Vector2.Lerp(c, vertices[f1], gradientRadiusModifier);
            Vector2 color2Position = Vector2.Lerp(c, vertices[f2], gradientRadiusModifier);
            Vector2 color3Position = Vector2.Lerp(c, vertices[f3], gradientRadiusModifier);

            //Gradient (solid if gradientRadiusModifier is 0)
            colors[f1] = texture.GetPixel((int)(color1Position.x), (int)(color1Position.y));
            colors[f2] = texture.GetPixel((int)(color2Position.x), (int)(color2Position.y));
            colors[f3] = texture.GetPixel((int)(color3Position.x), (int)(color3Position.y));
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// Calculate a triangel's inner circles center point by it's corner positions
    /// </summary>
    public static Vector2 TriangleCenter(Vector2 A, Vector2 B, Vector2 C)
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
}
