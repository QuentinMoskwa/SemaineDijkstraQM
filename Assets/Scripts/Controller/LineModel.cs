using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineModel : MonoBehaviour
{
    public Color neutralColor = Color.gray;

    public Color validColor = Color.green;

    public Color invalidColor = Color.red;

    public Color highlightColor = Color.yellow;


    public float weight;
    public Dictionary<string, Vector3> coordinates = new Dictionary<string, Vector3>();

    public void SetCoordinates(Vector3 start, Vector3 end)
    {
        coordinates["start"] = start;
        coordinates["end"] = end;
    }

    public void SetWeight(float newWeight, GameObject weightPrefab)
    {
        weight = newWeight;
        GetComponent<Renderer>().material.color = neutralColor;
        Vector3 middle = (coordinates["start"] + coordinates["end"]) / 2;
        GameObject weightGO = Instantiate(weightPrefab, middle, Quaternion.identity, transform);
        TextMeshProUGUI textMeshPro = null;

        foreach (Transform child in weightGO.transform)
        {
            foreach (Transform grandChild in child)
            {
                textMeshPro = grandChild.GetComponent<TextMeshProUGUI>();
                if (textMeshPro != null)
                {
                    textMeshPro.text = weight.ToString("F2");
                    break;
                }
            }
        }
    }

    public void SetValidColor()
    {
        GetComponent<Renderer>().material.color = validColor;
    }
    public void SetInvalidColor()
    {
        GetComponent<Renderer>().material.color = invalidColor;
    }

    public void SetHighlightColor()
    {
        GetComponent<Renderer>().material.color = highlightColor;
    }

    public void SetNeutralColor()
    {
        GetComponent<Renderer>().material.color = neutralColor;
    }
}
