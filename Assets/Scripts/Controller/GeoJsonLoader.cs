using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GeoJSONModel;

public class GeoJsonLoader : MonoBehaviour
{
    [Header("Fichier GeoJSON")]
    public TextAsset geoJsonFile;
    [Header("Graph Builder")]
    public GraphBuilder graphBuilder;

    [Header("Prefab pour les points")]
    public GameObject pointPrefab;

    [Header("Matériau pour les lignes")]
    public Material lineMaterial;

    [Header("Paramètres d'affichage")]
    public float lineWidth = 2f;

    public void LoadGeoJson()
    {
        if (geoJsonFile == null)
        {
            Debug.LogError("Aucun fichier GeoJSON assigné !");
            return;
        }

        FeatureCollection collection = JsonConvert.DeserializeObject<FeatureCollection>(geoJsonFile.text);
        if (collection == null || collection.Features == null)
        {
            Debug.LogError("Échec du parsing du GeoJSON.");
            return;
        }

        foreach (Feature feature in collection.Features)
        {
            if (feature.Geometry == null || string.IsNullOrEmpty(feature.Geometry.Type))
                continue;

            string geomType = feature.Geometry.Type;

            if (geomType.Equals("Point", StringComparison.OrdinalIgnoreCase))
            {
                CreatePoint(feature);
            }
            else if (geomType.Equals("LineString", StringComparison.OrdinalIgnoreCase))
            {
                CreateLine(feature);
            }
        }
        graphBuilder.BuildGraphFromGeoJSON(geoJsonFile);
    }

    void CreatePoint(Feature feature)
    {
        JArray coordsArray = feature.Geometry.Coordinates as JArray;
        if (coordsArray == null || coordsArray.Count < 2)
        {
            Debug.LogWarning("Coordonnées invalides pour le point : " + GetProperty(feature, "id"));
            return;
        }

        float x = coordsArray[0].ToObject<float>();
        float y = coordsArray[1].ToObject<float>();
        Vector3 pos = new Vector3(x, y, 0f);

        GameObject pointGO = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
        string name = GetProperty(feature, "name");
        if (!string.IsNullOrEmpty(name))
            pointGO.name = name;
            pointGO.GetComponent<PointModel>().pointName = name;
    }

    void CreateLine(Feature feature)
    {
        JArray coordsList = feature.Geometry.Coordinates as JArray;
        if (coordsList == null || coordsList.Count < 2)
        {
            Debug.LogWarning("Coordonnées invalides pour la ligne : " + GetProperty(feature, "id"));
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        foreach (var item in coordsList)
        {
            JArray point = item as JArray;
            if (point != null && point.Count >= 2)
            {
                float x = point[0].ToObject<float>();
                float y = point[1].ToObject<float>();
                positions.Add(new Vector3(x, y, 0f));
            }
        }

        if (positions.Count < 2)
        {
            Debug.LogWarning("Nombre insuffisant de points pour tracer la ligne : " + GetProperty(feature, "id"));
            return;
        }

        GameObject lineGO = new GameObject("Line_" + GetProperty(feature, "id"));
        lineGO.transform.parent = transform;
        LineRenderer lr = lineGO.AddComponent<LineRenderer>();
        lr.positionCount = positions.Count;
        lr.SetPositions(positions.ToArray());
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        LineModel lm = lineGO.AddComponent<LineModel>();
        string weight = GetProperty(feature, "weight");
        if (float.TryParse(weight, out float weightValue))
        {
            lm.SetWeight(weightValue);
        }
        else
        {
            Debug.LogWarning("Poids invalide pour la ligne : " + GetProperty(feature, "id"));
        }

    }

    string GetProperty(Feature feature, string key)
    {
        if (feature.Properties != null && feature.Properties.ContainsKey(key))
            return feature.Properties[key].ToString();
        return "";
    }
}
