using UnityEngine;
using Newtonsoft.Json;
using GeoJSONModel; // Assurez-vous que vos classes modèle (FeatureCollection, Feature, etc.) sont dans ce namespace

public class GeoJsonLoader : MonoBehaviour
{
    [Header("Fichier GeoJSON")]
    public TextAsset geoJsonFile;

    [HideInInspector]
    public FeatureCollection featureCollection;
    public GraphBuilder graphBuilder;


    public FeatureCollection LoadGeoJson()
    {
        if (geoJsonFile == null)
        {
            Debug.LogError("Aucun fichier GeoJSON assigné !");
            return null;
        }
        return featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoJsonFile.text);
    }
}
