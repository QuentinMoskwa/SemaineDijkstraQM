using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineModel : MonoBehaviour
{
    public Color neutralColor = Color.gray;
    // Start color
    public Color validColor = Color.green;
    // End color
    public Color invalidColor = Color.red;
    public float weight;

    public void SetWeight(float newWeight)
    {
        weight = newWeight;
        GetComponent<Renderer>().material.color = neutralColor;
    }
}
