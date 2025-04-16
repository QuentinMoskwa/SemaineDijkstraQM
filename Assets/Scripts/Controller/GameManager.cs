using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeoJSONModel;

public class GameManager : MonoBehaviour
{
    public DijkstraManager dijkstraManager;
    public GeoJsonLoader geoJsonLoader;
    public GeoJsonObjectCreator geoJsonObjectCreator;
    public PathGenerator pathGenerator;
    public GraphBuilder graphBuilder;
    public bool needToGeneratePath = false;
    public PointModel currentStart;
    public PointModel currentEnd;


    void Start()
    {
        geoJsonObjectCreator.CreateObjectsFromGeoJson(needToGeneratePath);
        if(needToGeneratePath)
        {
            pathGenerator.GeneratePaths();
            graphBuilder.BuildGraphFromGeneratedLines();
        }
        else
        {
            graphBuilder.BuildGraphFromGeoJson();
        }
    }

    public void SetCurrentStart(PointModel point)
    {
        currentStart = point;
    }

    public void SetCurrentEnd(PointModel point)
    {
        currentEnd = point;
    }

    public void OnStartButtonClicked()
    {
        if (currentStart != null && currentEnd != null)
        {
            if (dijkstraManager != null)
            {
                // dijkstraManager.StartComputePathWithSteps();
                dijkstraManager.ComputePath();
            }
            else
            {
                Debug.LogError("dijkstraManager not found in the scene.");
            }
        }
        else
        {
            Debug.LogWarning("Point de départ ou d'arrivée non défini.");
        }
    }
}
