using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    GameManager gameManager;
    [Header("Prefab pour le menu")]
    public GameObject menuPrefab;
    public TextMeshProUGUI cityNameText;
    public PointModel selectedPoint;
    public PointModel previousPoint;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if(gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }
        ClosePointMenu();
    }


    public void OpenPointMenu(GameObject point)
    {
        selectedPoint = point.GetComponent<PointModel>();
        if(previousPoint != null && previousPoint != gameManager.currentStart && previousPoint != gameManager.currentEnd)
        {
            previousPoint.SetNeutralColor();
        }
        Debug.Log("Ouverture du menu pour le point : " + selectedPoint.transform.name);
        if (selectedPoint == null)
        {
            Debug.LogWarning("Clicked object does not have a PointModel component.");
            return;
        }
        string cityName = selectedPoint.pointName;
        cityNameText.text = cityName;
        menuPrefab.SetActive(true);
        if(selectedPoint != gameManager.currentStart && selectedPoint != gameManager.currentEnd)
        {
            selectedPoint.SetHighlightColor();
        }
        previousPoint = selectedPoint;
    }

    void ClosePointMenu()
    {
        menuPrefab.SetActive(false);
    }

    public void SetAsStart()
    {
        if (gameManager.currentStart != null && gameManager.currentStart != selectedPoint)
        {
            gameManager.currentStart.SetNeutralColor();
        }
        gameManager.SetCurrentStart(selectedPoint);
        selectedPoint.SetStartColor();
    }

    public void SetAsEnd()
    {
        if (gameManager.currentEnd != null && gameManager.currentEnd != selectedPoint)
        {
            gameManager.currentEnd.SetNeutralColor();
        }
        gameManager.SetCurrentEnd(selectedPoint);
        selectedPoint.SetEndColor();
    }

}
