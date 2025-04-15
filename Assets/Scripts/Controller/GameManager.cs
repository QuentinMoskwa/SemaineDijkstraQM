using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GeoJsonLoader geoJsonLoader; // Référence au script GeoJsonLoader
    public PointModel currentStart;
    public PointModel currentEnd;


    void Start()
    {
        geoJsonLoader.LoadGeoJson();
    }

    public void SetCurrentStart(PointModel point)
    {
        currentStart = point;
    }

    public void SetCurrentEnd(PointModel point)
    {
        currentEnd = point;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
