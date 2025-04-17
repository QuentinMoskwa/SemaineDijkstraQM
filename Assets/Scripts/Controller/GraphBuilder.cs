using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GraphBuilder : MonoBehaviour
{
    public GeoJsonLoader geoJsonFileLoader;

    public Dictionary<PointModel, Dictionary<PointModel, float>> graph = new();

    private Dictionary<Vector2, PointModel> coordToCity = new();

    public List<LineModel> lines = new List<LineModel>();

    [Header("Sphere Settings for Projection")]
    [Tooltip("Diamètre utilisé pour la projection (doit être le même que pour créer les points)")]
    public float earthDiameter = 20f;

    [Tooltip("Centre de la sphère")]
    public Vector3 sphereCenter = Vector3.zero;


    public void AddCity(PointModel city, Vector2 coords)
    {
        if (city == null)
        {
            Debug.LogWarning("Tentative d'ajouter une ville null dans le graphe.");
            return;
        }
        Vector2 key = new Vector2(Mathf.Round(coords.x * 1000f) / 1000f, Mathf.Round(coords.y * 1000f) / 1000f);
        if (!coordToCity.ContainsKey(key))
        {
            coordToCity[key] = city;
        }
        if (!graph.ContainsKey(city))
        {
            graph[city] = new Dictionary<PointModel, float>();
        }
    }

    public void AddLine(LineModel line)
    {
        if (line != null && !lines.Contains(line))
        {
            lines.Add(line);
        }
    }

    public void BuildGraphFromGeoJson()
    {
        if (geoJsonFileLoader == null)
        {
            Debug.LogError("GeoJsonLoader non assigné dans GraphBuilder.");
            return;
        }

        var collection = geoJsonFileLoader.featureCollection;
        if (collection == null || collection.Features == null)
        {
            Debug.LogError("La FeatureCollection est null dans le GeoJsonLoader.");
            return;
        }

        foreach (var feature in collection.Features)
        {
            string geomType = feature.Geometry?.Type;
            if (geomType != "LineString")
                continue;

            JArray coords = feature.Geometry.Coordinates as JArray;
            if (coords == null || coords.Count != 2)
                continue;

            float lon1 = coords[0][0].ToObject<float>();
            float lat1 = coords[0][1].ToObject<float>();
            float lon2 = coords[1][0].ToObject<float>();
            float lat2 = coords[1][1].ToObject<float>();

            Vector3 pos1 = GeoJsonObjectCreator.LatLonToSpherePosition(lat1, lon1, earthDiameter, sphereCenter);
            Vector3 pos2 = GeoJsonObjectCreator.LatLonToSpherePosition(lat2, lon2, earthDiameter, sphereCenter);

            Vector2 proj1 = new Vector2(pos1.x, pos1.z);
            Vector2 proj2 = new Vector2(pos2.x, pos2.z);
            proj1 = new Vector2(Mathf.Round(proj1.x * 1000f) / 1000f, Mathf.Round(proj1.y * 1000f) / 1000f);
            proj2 = new Vector2(Mathf.Round(proj2.x * 1000f) / 1000f, Mathf.Round(proj2.y * 1000f) / 1000f);

            if (coordToCity.TryGetValue(proj1, out PointModel cityA) && coordToCity.TryGetValue(proj2, out PointModel cityB))
            {
                if (!graph.ContainsKey(cityA)) graph[cityA] = new Dictionary<PointModel, float>();
                if (!graph.ContainsKey(cityB)) graph[cityB] = new Dictionary<PointModel, float>();

                float weight = ComputeArcDistance(pos1, pos2);
                if (!graph[cityA].ContainsKey(cityB))
                    graph[cityA][cityB] = weight;
                if (!graph[cityB].ContainsKey(cityA))
                    graph[cityB][cityA] = weight;
            }
            else
            {
                Debug.LogWarning("Coordonnées non reconnues pour une liaison.");
            }
        }
    }


    private float ComputeArcDistance(Vector3 posA, Vector3 posB)
    {
        Vector3 dirA = (posA - sphereCenter).normalized;
        Vector3 dirB = (posB - sphereCenter).normalized;
        float angleRad = Vector3.Angle(dirA, dirB) * Mathf.Deg2Rad;
        return (earthDiameter / 2f) * angleRad;
    }

    public LineModel GetLineBetween(PointModel a, PointModel b)
    {
        foreach (var line in lines)
        {
            Vector3 startPos = line.coordinates.ContainsKey("start") ? line.coordinates["start"] : Vector3.zero;
            Vector3 endPos = line.coordinates.ContainsKey("end") ? line.coordinates["end"] : Vector3.zero;
            if ((ApproximatelyEqual(startPos, a.transform.position) && ApproximatelyEqual(endPos, b.transform.position)) ||
                (ApproximatelyEqual(startPos, b.transform.position) && ApproximatelyEqual(endPos, a.transform.position)))
            {
                return line;
            }
        }
        return null;
    }

    public void BuildGraphFromGeneratedLines()
    {
        foreach (LineModel lm in lines)
        {
            if (lm.cityA != null && lm.cityB != null)
            {
                float distance = ComputeArcDistance(lm.cityA.transform.position, lm.cityB.transform.position);
                if (!graph.ContainsKey(lm.cityA))
                    graph[lm.cityA] = new Dictionary<PointModel, float>();
                if (!graph.ContainsKey(lm.cityB))
                    graph[lm.cityB] = new Dictionary<PointModel, float>();

                graph[lm.cityA][lm.cityB] = distance;
                graph[lm.cityB][lm.cityA] = distance;
            }
        }
    }

    private bool ApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.01f)
    {
        return Vector3.Distance(a, b) < tolerance;
    }
}
