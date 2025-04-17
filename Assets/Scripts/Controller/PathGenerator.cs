using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PathGenerator : MonoBehaviour
{
    [Header("References")]
    public GraphBuilder graphBuilder;
    public GameObject weightPrefab;
    [Header("Parent for generated paths")]
    public Transform parentPaths;
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
    [Tooltip("Nombre de connexions par ville (chemins les plus proches)")]
    public int pathsPerCity = 1;

    public void GeneratePaths()
    {
        if (graphBuilder == null || graphBuilder.graph == null || graphBuilder.graph.Count == 0)
        {
            Debug.LogError("GraphBuilder ou graph non initialisé !");
            return;
        }

        List<PointModel> cities = new List<PointModel>(graphBuilder.graph.Keys);
        HashSet<string> createdPaths = new HashSet<string>();

        foreach (PointModel city in cities)
        {
            List<(PointModel city, float distance)> nearestCities = new List<(PointModel, float)>();

            foreach (PointModel other in cities)
            {
                if (other == city) continue;

                float distance = ComputeArcDistance(city.transform.position, other.transform.position);
                nearestCities.Add((other, distance));
            }

            nearestCities.Sort((a, b) => a.distance.CompareTo(b.distance));

            int connectionsMade = 0;
            foreach (var (otherCity, distance) in nearestCities)
            {
                if (connectionsMade >= pathsPerCity)
                    break;

                string pathKey = GetPathKey(city, otherCity);
                if (!createdPaths.Contains(pathKey))
                {
                    CreatePathBetween(city, otherCity, distance);
                    createdPaths.Add(pathKey);

                    if (!graphBuilder.graph[city].ContainsKey(otherCity))
                        graphBuilder.graph[city][otherCity] = distance;
                    if (!graphBuilder.graph[otherCity].ContainsKey(city))
                        graphBuilder.graph[otherCity][city] = distance;

                    connectionsMade++;
                }
            }
        }
    }


    private string GetPathKey(PointModel a, PointModel b)
    {
        return a.name.CompareTo(b.name) < 0 ? $"{a.name}_{b.name}" : $"{b.name}_{a.name}";
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
        // si la ville est Manizales
        // if (cityA.name == "Manizales" || cityB.name == "Manizales")
        // {
        //     Debug.Log("Création d'un chemin entre " + cityA.name + " et " + cityB.name);
        // }
        // On récupère la position des villes (logique)
        Vector3 startPos = cityA.transform.position;
        Vector3 endPos = cityB.transform.position;

        // On calcule les points interpolés pour dessiner un arc, mais ceux-ci ne seront pas utilisés pour l'identification
        List<Vector3> positions = new List<Vector3>();
        Vector3 startDir = (startPos - sphereCenter).normalized;
        Vector3 endDir = (endPos - sphereCenter).normalized;
        for (int i = 0; i <= lineSegmentCount; i++)
        {
            float t = (float)i / lineSegmentCount;
            Vector3 pointOnArc = sphereCenter + Vector3.Slerp(startDir, endDir, t) * (sphereRadius / 2f);
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

        LineModel lm = lineGO.AddComponent<LineModel>();
        lm.SetCoordinates(startPos, endPos);
        lm.SetWeight(distance, weightPrefab);
        lm.cityA = cityA;
        lm.cityB = cityB;

        graphBuilder.AddLine(lm);
    }
}
