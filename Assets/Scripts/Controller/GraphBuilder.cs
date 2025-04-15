using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GraphBuilder : MonoBehaviour
{
    public Dictionary<PointModel, Dictionary<PointModel, float>> graph = new();

    private Dictionary<Vector2, PointModel> coordToCity = new();

    public List<LineModel> lines = new List<LineModel>();

    public void AddCity(PointModel city, Vector2 coords)
    {
        if (city == null)
        {
            Debug.LogWarning("Tentative d'ajouter une ville null dans le graphe.");
            return;
        }
        if (!coordToCity.ContainsKey(coords))
        {
            coordToCity[coords] = city;
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
        // PrintGraph();
    }

    // void PrintGraph()
    // {
    //     Debug.Log("====== Graphe des connexions entre villes ======");
    //     foreach (var city in graph)
    //     {
    //         string connections = $"{city.Key.gameObject.name} : ";
    //         foreach (var target in city.Value)
    //         {
    //             connections += $"{target.Key.gameObject.name} ({target.Value})  ";
    //         }
    //         Debug.Log(connections);
    //     }
    // }

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
