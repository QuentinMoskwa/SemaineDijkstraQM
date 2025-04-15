using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GraphBuilder : MonoBehaviour
{
    // Graphe indexé par des PointModel
    public Dictionary<PointModel, Dictionary<PointModel, float>> graph = new();

    // Mapping des points via une projection sur le plan XZ
    private Dictionary<Vector2, PointModel> coordToCity = new();

    public List<LineModel> lines = new List<LineModel>();


    public void AddLine(LineModel line)
    {
        if (line != null && !lines.Contains(line))
        {
            lines.Add(line);
        }
    }

    public void BuildGraphFromGeoJSON(TextAsset geoJsonFile)
    {
        JObject root = JObject.Parse(geoJsonFile.text);
        JArray features = root["features"] as JArray;
        if (features == null)
            return;

        foreach (var feature in features)
        {
            string geomType = feature["geometry"]?["type"]?.ToString();
            if (geomType == "LineString")
            {
                JArray coords = feature["geometry"]?["coordinates"] as JArray;
                float weight = feature["properties"]?["weight"]?.ToObject<float>() ?? -1;
                if (coords != null && coords.Count == 2)
                {
                    // On projette sur le plan XZ, en supposant que notre conversion lat/lon a déjà été faite.
                    Vector2 posA = new Vector2(coords[0][0].ToObject<float>(), coords[0][1].ToObject<float>());
                    Vector2 posB = new Vector2(coords[1][0].ToObject<float>(), coords[1][1].ToObject<float>());

                    if (coordToCity.TryGetValue(posA, out PointModel cityA) &&
                        coordToCity.TryGetValue(posB, out PointModel cityB))
                    {
                        if (!graph.ContainsKey(cityA)) graph[cityA] = new Dictionary<PointModel, float>();
                        if (!graph.ContainsKey(cityB)) graph[cityB] = new Dictionary<PointModel, float>();

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
        }
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

    private bool ApproximatelyEqual(Vector3 a, Vector3 b, float tolerance = 0.01f)
    {
        return Vector3.Distance(a, b) < tolerance;
    }
}
