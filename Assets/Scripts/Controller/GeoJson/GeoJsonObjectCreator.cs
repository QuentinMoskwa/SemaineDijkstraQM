using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using GeoJSONModel;

public class GeoJsonObjectCreator : MonoBehaviour
{
    [Header("Références au loader")]
    public GeoJsonLoader geoJsonLoader;
    [Header("Références au graphBuilder")]
    public GraphBuilder graphBuilder;

    [Header("Parent des villes et lignes")]
    [Tooltip("Transform qui sera le parent de tous les points (villes) créés")]
    public Transform parentCities;
    [Tooltip("Transform qui sera le parent de toutes les lignes créées")]
    public Transform parentLines;

    [Header("Préfab pour les points")]
    public GameObject pointPrefab;
    [Header("Echelle des points")]
    public float pointScale = 0.5f;

    [Header("Matériau pour les lignes")]
    public Material lineMaterial;
    [Header("Prefab pour l'affichage du poids")]
    public GameObject weightPrefab;
    [Header("Paramètres d'affichage des lignes")]
    public float lineWidth = 0.2f;
    public float lineScale = 1f;
    public int lineSegmentCount = 20; // Pour les arcs courbés

    [Header("Paramètres de la Terre")]
    [Tooltip("Diamètre de la sphère représentant la Terre (en unités Unity)")]
    public float earthDiameter = 20f;


    public void CreateObjectsFromGeoJson()
    {
        if (geoJsonLoader == null || geoJsonLoader.featureCollection == null)
        {
            Debug.LogError("La FeatureCollection n'est pas disponible.");
            return;
        }

        FeatureCollection collection = geoJsonLoader.featureCollection;
        foreach (Feature feature in collection.Features)
        {
            if (feature.Geometry == null || string.IsNullOrEmpty(feature.Geometry.Type))
                continue;

            string geomType = feature.Geometry.Type;
            if (geomType.Equals("Point", StringComparison.OrdinalIgnoreCase))
            {
                CreatePoint(feature);
            }
            // else if (geomType.Equals("LineString", StringComparison.OrdinalIgnoreCase))
            // {
            //     CreateLine(feature);
            // }
        }
    }

    void CreatePoint(Feature feature)
    {
        JArray coordsArray = feature.Geometry.Coordinates as JArray;
        if (coordsArray == null || coordsArray.Count < 2)
        {
            Debug.LogWarning("Coordonnées invalides pour le point : " + GetProperty(feature, "id"));
            return;
        }

        // On suppose le format [longitude, latitude]
        float lon = coordsArray[0].ToObject<float>();
        float lat = coordsArray[1].ToObject<float>();
        Vector3 pos = LatLonToSpherePosition(lat, lon, earthDiameter, Vector3.zero);

        // Instanciation du point
        GameObject pointGO = Instantiate(pointPrefab, pos, Quaternion.identity);
        // On utilise le parent défini, sinon on utilise le transform courant
        Transform parentToUse = (parentCities != null) ? parentCities : this.transform;
        pointGO.transform.SetParent(parentToUse, false);
        pointGO.transform.localScale = Vector3.one * pointScale;

        string name = GetProperty(feature, "name");
        if (!string.IsNullOrEmpty(name))
        {
            pointGO.name = name;
            PointModel pm = pointGO.GetComponent<PointModel>();
            if (pm != null)
            {
                pm.pointName = name;
            }
            // Enregistrement dans le GraphBuilder
            if (geoJsonLoader != null && geoJsonLoader.featureCollection != null && graphBuilder != null)
            {
                Vector2 pos2D = new Vector2(pos.x, pos.z);
                // graphBuilder.AddCity(pointGO.GetComponent<PointModel>(), pos2D);
            }
        }
    }

    void CreateLine(Feature feature)
    {
        JArray coordsList = feature.Geometry.Coordinates as JArray;
        if (coordsList == null || coordsList.Count < 2)
        {
            Debug.LogWarning("Coordonnées invalides pour la ligne : " + GetProperty(feature, "id"));
            return;
        }

        // On prend le premier et le dernier élément pour définir l'arc
        JArray startCoords = coordsList.First as JArray;
        JArray endCoords = coordsList.Last as JArray;
        if (startCoords == null || endCoords == null)
        {
            Debug.LogWarning("Données invalides dans la ligne : " + GetProperty(feature, "id"));
            return;
        }

        float startLon = startCoords[0].ToObject<float>();
        float startLat = startCoords[1].ToObject<float>();
        float endLon = endCoords[0].ToObject<float>();
        float endLat = endCoords[1].ToObject<float>();

        Vector3 startPos = LatLonToSpherePosition(startLat, startLon, earthDiameter, Vector3.zero);
        Vector3 endPos = LatLonToSpherePosition(endLat, endLon, earthDiameter, Vector3.zero);

        // Interpoler sur le grand cercle à l'aide de Slerp
        List<Vector3> positions = new List<Vector3>();
        Vector3 startDir = (startPos - Vector3.zero).normalized;
        Vector3 endDir = (endPos - Vector3.zero).normalized;
        for (int i = 0; i <= lineSegmentCount; i++)
        {
            float t = (float)i / lineSegmentCount;
            Vector3 pointOnArc = Vector3.zero + Vector3.Slerp(startDir, endDir, t) * earthDiameter;
            positions.Add(pointOnArc);
        }

        if (positions.Count < 2)
        {
            Debug.LogWarning("Nombre insuffisant de points pour tracer la ligne : " + GetProperty(feature, "id"));
            return;
        }

        GameObject lineGO = new GameObject("Line_" + GetProperty(feature, "id"));
        Transform parentToUse = (parentLines != null) ? parentLines : this.transform;
        lineGO.transform.SetParent(parentToUse, false);
        lineGO.transform.localScale = Vector3.one * lineScale;

        LineRenderer lr = lineGO.AddComponent<LineRenderer>();
        lr.positionCount = positions.Count;
        lr.SetPositions(positions.ToArray());
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        LineModel lm = lineGO.AddComponent<LineModel>();
        lm.SetCoordinates(positions[0], positions[positions.Count - 1]);

        string weight = GetProperty(feature, "weight");
        if (float.TryParse(weight, out float weightValue))
        {
            lm.SetWeight(weightValue, weightPrefab);
        }
        else
        {
            Debug.LogWarning("Poids invalide pour la ligne : " + GetProperty(feature, "id"));
        }

        // Ajoutez éventuellement la ligne au GraphBuilder
        if (graphBuilder != null)
        {
            graphBuilder.AddLine(lm);
        }
    }

    string GetProperty(Feature feature, string key)
    {
        if (feature.Properties != null && feature.Properties.ContainsKey(key))
            return feature.Properties[key].ToString();
        return "";
    }

    public static Vector3 LatLonToSpherePosition(float latDeg, float lonDeg, float diameter, Vector3 center)
    {
        float radius = diameter / 2f;
        float latRad = latDeg * Mathf.Deg2Rad;
        float lonRad = lonDeg * Mathf.Deg2Rad;
        float x = radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float y = radius * Mathf.Sin(latRad);
        float z = radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);
        return center + new Vector3(x, y, z);
    }
}
