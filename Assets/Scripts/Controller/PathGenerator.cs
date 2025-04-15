using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    [Header("References")]
    public GraphBuilder graphBuilder;   // Must already contain all cities in graphBuilder.graph
    [Header("Parent for generated paths")]
    public Transform parentPaths;       // Public object to choose as parent for all paths
    [Header("Line Settings")]
    [Tooltip("Maximum arc distance to create a connection (in Unity units)")]
    public float thresholdDistance = 300f;
    [Tooltip("Width of the generated lines")]
    public float lineWidth = 0.2f;
    [Tooltip("Number of segments to interpolate the arc (more segments = smoother arc)")]
    public int lineSegmentCount = 20;
    [Header("Sphere Settings")]
    [Tooltip("Center of the sphere")]
    public Vector3 sphereCenter = Vector3.zero;
    [Tooltip("Radius of the sphere")]
    public float sphereRadius = 10f;
    [Header("Line Material")]
    public Material lineMaterial;

    // TODO GROS PROBLEME POUR GENERER LES CHEMINS 
    public void GeneratePaths()
    {
        if (graphBuilder == null)
        {
            Debug.LogError("GraphBuilder is null!");
            return;
        }
        if (graphBuilder.graph == null)
        {
            // If graph is not yet instantiated, create an empty one
            graphBuilder.graph = new Dictionary<PointModel, Dictionary<PointModel, float>>();
        }
        if (graphBuilder.graph.Count == 0)
        {
            Debug.LogError("Graph is empty! Ensure that city objects are generated first.");
            return;
        }
        else
        {
            Debug.Log("Graph is defined: " + graphBuilder.graph.Count + " cities found.");
        }

        List<PointModel> cities = new List<PointModel>(graphBuilder.graph.Keys);
        // Loop through each pair of cities
        for (int i = 0; i < cities.Count; i++)
        {
            for (int j = i + 1; j < cities.Count; j++)
            {
                PointModel cityA = cities[i];
                PointModel cityB = cities[j];

                float distance = ComputeArcDistance(cityA.transform.position, cityB.transform.position);
                if (distance <= thresholdDistance)
                {
                    // Create the visual connection (arc) between the two cities
                    CreatePathBetween(cityA, cityB, distance);
                    
                    // Add the connection to the graph in both directions
                    if (!graphBuilder.graph[cityA].ContainsKey(cityB))
                        graphBuilder.graph[cityA][cityB] = distance;
                    if (!graphBuilder.graph[cityB].ContainsKey(cityA))
                        graphBuilder.graph[cityB][cityA] = distance;
                }
            }
        }
    }

    private float ComputeArcDistance(Vector3 posA, Vector3 posB)
    {
        Vector3 dirA = (posA - sphereCenter).normalized;
        Vector3 dirB = (posB - sphereCenter).normalized;
        float angleRad = Vector3.Angle(dirA, dirB) * Mathf.Deg2Rad;
        return sphereRadius * angleRad;
    }

    private void CreatePathBetween(PointModel cityA, PointModel cityB, float distance)
    {
        Vector3 startPos = cityA.transform.position;
        Vector3 endPos = cityB.transform.position;
        List<Vector3> positions = new List<Vector3>();

        // Get directions from the sphere center to the positions
        Vector3 startDir = (startPos - sphereCenter).normalized;
        Vector3 endDir = (endPos - sphereCenter).normalized;
        for (int i = 0; i <= lineSegmentCount; i++)
        {
            float t = (float)i / lineSegmentCount;
            // Use spherical interpolation (Slerp) to follow the curvature of the sphere
            Vector3 pointOnArc = sphereCenter + Vector3.Slerp(startDir, endDir, t) * sphereRadius;
            positions.Add(pointOnArc);
        }

        if (positions.Count < 2)
        {
            Debug.LogWarning("Insufficient points to draw the line.");
            return;
        }

        GameObject lineGO = new GameObject("Path_" + cityA.name + "_" + cityB.name);
        Transform parentToUse = (parentPaths != null) ? parentPaths : graphBuilder.transform;
        lineGO.transform.SetParent(parentToUse, false);

        LineRenderer lr = lineGO.AddComponent<LineRenderer>();
        lr.positionCount = positions.Count;
        lr.SetPositions(positions.ToArray());
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        LineModel lm = lineGO.GetComponent<LineModel>();
        if (lm == null)
            lm = lineGO.AddComponent<LineModel>();
        lm.SetCoordinates(positions[0], positions[positions.Count - 1]);
        lm.SetWeight(distance, null); 

        lm.cityA = cityA;
        lm.cityB = cityB;

        graphBuilder.AddLine(lm);
    }
}
