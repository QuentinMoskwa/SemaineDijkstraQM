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
    public UIManager uiManager;
    public bool needToGeneratePath = false;
    public PointModel currentStart;
    public PointModel currentEnd;
    bool dijkstraRunning = false;


    void Start()
    {
        if(dijkstraManager != null)
        {
            dijkstraManager.OnDijkstraComplete += HandleDijkstraComplete;
        }
        

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

    private void HandleDijkstraComplete(float finalDistance)
    {
        uiManager.ShowFinalDistance(finalDistance);
    }


    public void SetCurrentStart(PointModel point)
    {
        currentStart = point;
    }

    public void SetCurrentEnd(PointModel point)
    {
        currentEnd = point;
    }

    public void OnStartButtonClicked(bool isStepByStep)
    {
        if (currentStart != null && currentEnd != null)
        {
            if (dijkstraManager != null)
            {
                uiManager.ShowReset();
                if(!isStepByStep)
                {
                    dijkstraManager.ComputePath();
                    uiManager.HideStepByStep();
                }
                else
                {
                    if(!dijkstraRunning)
                    {
                        uiManager.HideStart();
                        uiManager.ShowReset();
                        dijkstraRunning = true;
                        dijkstraManager.ComputePathStepByStep();
                    }
                    else
                    {
                        dijkstraManager.OnNextStepButtonClicked();
                    }
                }
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

    public void OnResetButtonClicked()
    {
        uiManager.HideEverything();
        dijkstraManager.StopDijkstra();
        dijkstraRunning = false;
        currentStart = null;
        currentEnd = null;
    }
}
