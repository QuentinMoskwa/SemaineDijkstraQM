using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointModel : MonoBehaviour
{
    public Color neutralColor = Color.gray;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    public Color highlightColor = Color.yellow;
    public string pointName = "Point";
    public enum PointState
    {
        Neutral,
        Start,
        End
    }

    public PointState pointState = PointState.Neutral;

    void Start()
    {
        GetComponent<Renderer>().material.color = neutralColor;
    }

    public void SetStartColor()
    {
        pointState = PointState.Start;
        GetComponent<Renderer>().material.color = startColor;
    }

    public void SetEndColor()
    {
        pointState = PointState.End;
        GetComponent<Renderer>().material.color = endColor;
    }
    public void SetNeutralColor()
    {
        pointState = PointState.Neutral;
        GetComponent<Renderer>().material.color = neutralColor;
    }
    public void SetHighlightColor()
    {
        GetComponent<Renderer>().material.color = highlightColor;
    }

}
