using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GraphBuilder : MonoBehaviour
{
    public Dictionary<string, Dictionary<string, float>> graph = new();

    private Dictionary<Vector2, string> coordToCity = new();


    public void BuildGraphFromGeoJSON(TextAsset geoJsonFile)
    {
        JObject root = JObject.Parse(geoJsonFile.text);
        JArray features = root["features"] as JArray;

        if (features == null) return;

        foreach (var feature in features)
        {
            string geomType = feature["geometry"]?["type"]?.ToString();

            if (geomType == "Point")
            {
                string cityName = feature["properties"]?["name"]?.ToString();
                JArray coords = feature["geometry"]?["coordinates"] as JArray;
                if (coords != null && cityName != null)
                {
                    float x = coords[0].ToObject<float>();
                    float y = coords[1].ToObject<float>();
                    Vector2 pos = new Vector2(x, y);

                    if (!coordToCity.ContainsKey(pos))
                        coordToCity[pos] = cityName;

                    if (!graph.ContainsKey(cityName))
                        graph[cityName] = new Dictionary<string, float>();
                }
            }
        }

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

                    if (coordToCity.TryGetValue(posA, out string nameA) && coordToCity.TryGetValue(posB, out string nameB))
                    {
                        if (!graph.ContainsKey(nameA)) graph[nameA] = new();
                        if (!graph.ContainsKey(nameB)) graph[nameB] = new();

                        if (!graph[nameA].ContainsKey(nameB))
                            graph[nameA][nameB] = weight;

                        if (!graph[nameB].ContainsKey(nameA))
                            graph[nameB][nameA] = weight; // graphe non-orienté
                    }
                    else
                    {
                        Debug.LogWarning("Coordonnées non reconnues pour une liaison.");
                    }
                }
            }
        }
        PrintGraph();
    }

    void PrintGraph()
    {
        Debug.Log("====== Graphe des connexions entre villes ======");
        foreach (var city in graph)
        {
            string connections = $"{city.Key} : ";
            foreach (var target in city.Value)
            {
                connections += $"{target.Key} ({target.Value})  ";
            }
            Debug.Log(connections);
        }
    }
}
