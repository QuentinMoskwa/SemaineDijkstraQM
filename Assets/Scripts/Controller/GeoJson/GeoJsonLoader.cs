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


    public void LoadGeoJson()
    {
        if (geoJsonFile == null)
        {
            Debug.LogError("Aucun fichier GeoJSON assigné !");
            return;
        }
        featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(geoJsonFile.text);
        if (featureCollection == null || featureCollection.Features == null)
        {
            Debug.LogError("Échec du parsing du GeoJSON.");
        }
        else
        {
            Debug.Log("GeoJSON chargé avec succès. Nombre de features : " + featureCollection.Features.Count);
            graphBuilder.BuildGraphFromGeoJSON(geoJsonFile);
        }
    }


}
