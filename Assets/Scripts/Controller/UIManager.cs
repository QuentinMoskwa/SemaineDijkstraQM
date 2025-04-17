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
    public TextMeshProUGUI startCityText;
    public TextMeshProUGUI endCityText;
    public TextMeshProUGUI finalDistanceText;
    public PointModel selectedPoint;
    public PointModel previousPoint;
    public GameObject startButton;
    public GameObject resetButton;
    public GameObject stepByStepButton;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if(gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }
        HideEverything();
    }


    public void OpenPointMenu(GameObject point)
    {
        selectedPoint = point.GetComponent<PointModel>();
        if(previousPoint != null && previousPoint != gameManager.currentStart && previousPoint != gameManager.currentEnd)
        {
            previousPoint.SetNeutralColor();
        }
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

    public void ClosePointMenu()
    {
        menuPrefab.SetActive(false);
    }

    public void SetAsStart()
    {
        if (gameManager.currentStart != null && gameManager.currentStart != selectedPoint)
        {
            gameManager.currentStart.SetNeutralColor();
        }
        if (!resetButton.activeSelf)
        {
            resetButton.SetActive(true);
        }
        gameManager.SetCurrentStart(selectedPoint);
        selectedPoint.SetStartColor();
        SetStartCityText(selectedPoint.pointName);
        CheckIfStartAndEndSet();
    }

    public void SetAsEnd()
    {
        if (gameManager.currentEnd != null && gameManager.currentEnd != selectedPoint)
        {
            gameManager.currentEnd.SetNeutralColor();
        }
        if(!resetButton.activeSelf)
        {
            resetButton.SetActive(true);
        }
        gameManager.SetCurrentEnd(selectedPoint);
        selectedPoint.SetEndColor();
        SetEndCityText(selectedPoint.pointName);
        CheckIfStartAndEndSet();
    }

    void CheckIfStartAndEndSet()
    {
        if (gameManager.currentStart != null && gameManager.currentEnd != null)
        {
            ShowEverything();
        }
    }

    public void HideStart()
    {
        startButton.SetActive(false);
    }

    public void ShowStart()
    {
        startButton.SetActive(true);
    }

    public void HideReset()
    {
        resetButton.SetActive(false);
    }

    public void ShowReset()
    {
        resetButton.SetActive(true);
    }

    public void HideStepByStep()
    {
        stepByStepButton.SetActive(false);
    }

    public void ShowStepByStep()
    {
        stepByStepButton.SetActive(true);
    }

    public void ShowEverything()
    {
        startButton.SetActive(true);
        resetButton.SetActive(true);
        stepByStepButton.SetActive(true);
    }
    public void HideEverything()
    {
        finalDistanceText.transform.parent.gameObject.SetActive(false);
        startButton.SetActive(false);
        resetButton.SetActive(false);
        stepByStepButton.SetActive(false);
        menuPrefab.SetActive(false);
        ClearCityText();
    }

    public void ShowFinalDistance(float distance)
    {
        finalDistanceText.transform.parent.gameObject.SetActive(true);
        finalDistanceText.text = "Distance : " + distance.ToString("F2") + " km";
    }

    public void SetStartCityText(string cityName)
    {
        startCityText.text = cityName;
    }

    public void SetEndCityText(string cityName)
    {
        endCityText.text = cityName;
    }

    public void ClearCityText()
    {
        startCityText.text = "???";
        endCityText.text = "???";
    }

}
