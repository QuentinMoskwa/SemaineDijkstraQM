using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using GeoJSONModel;

public class GeoJsonLoader : MonoBehaviour
{
    [Header("Nom du fichier JSON (sans extension)")]
    string fileName = "GeoJson"; // sans .json

    [Tooltip("Utiliser StreamingAssets (true) ou persistentDataPath (false)")]
    public bool useStreamingAssets = true;

    [HideInInspector]
    public FeatureCollection featureCollection;

    public GraphBuilder graphBuilder;

    public FeatureCollection LoadGeoJson()
    {
        string path;

        if (useStreamingAssets)
        {
            path = Path.Combine(Application.streamingAssetsPath, fileName + ".json");
        }
        else
        {
            path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        }

        if (!File.Exists(path))
        {
            Debug.LogError($"Le fichier JSON n'existe pas à ce chemin : {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json);
            Debug.Log("GeoJSON chargé depuis : " + path);
            return featureCollection;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Erreur lors du chargement du GeoJSON : " + ex.Message);
            return null;
        }
    }
}
