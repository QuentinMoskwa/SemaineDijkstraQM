using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraManager : MonoBehaviour
{
    public GraphBuilder graphBuilder;
    public GameManager gameManager;

    [Header("Mode Debug & Step-by-Step")]
    public bool debugMode = true;
    public bool nextStep = false;

    public void ComputePath()
    {
        PointModel start = gameManager.currentStart;
        PointModel end = gameManager.currentEnd;
        var graph = graphBuilder.graph;

        var distances = new Dictionary<PointModel, float>();
        var previous = new Dictionary<PointModel, PointModel>();
        var toVisit = new List<PointModel>();

        foreach (var city in graph.Keys)
        {
            distances[city] = float.MaxValue;
            toVisit.Add(city);
        }
        distances[start] = 0f;

        while (toVisit.Count > 0)
        {
            PointModel current = null;
            float minDist = float.MaxValue;
            foreach (var city in toVisit)
            {
                if (distances[city] < minDist)
                {
                    minDist = distances[city];
                    current = city;
                }
            }
            if (current == null)
                break;
            toVisit.Remove(current);
            if (current == end)
                break;

            foreach (var neighbor in graph[current])
            {
                float newDist = distances[current] + neighbor.Value;
                if (newDist < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = newDist;
                    previous[neighbor.Key] = current;
                }
            }
        }

        List<PointModel> path = new List<PointModel>();
        PointModel step = end;
        while (previous.ContainsKey(step))
        {
            path.Insert(0, step);
            step = previous[step];
        }
        if (step == start)
        {
            path.Insert(0, start);
            string pathStr = "";
            foreach (var pt in path)
            {
                pathStr += pt.name + " → ";
            }
            pathStr = pathStr.TrimEnd(' ', '→');
            Debug.Log("Chemin trouvé : " + pathStr);
            Debug.Log("Coût total : " + distances[end]);

            for (int i = 0; i < path.Count - 1; i++)
            {
                LineModel line = graphBuilder.GetLineBetween(path[i], path[i + 1]);
                if (line != null)
                {
                    line.SetHighlightColor();
                }
                else
                {
                    Debug.LogWarning("Aucune ligne trouvée entre " + path[i].name + " et " + path[i + 1].name);
                }
            }

            foreach (PointModel pt in path)
            {
                if(pt != start && pt != end)
                {
                    pt.SetHighlightColor();
                }
            }
        }
        else
        {
            Debug.LogWarning("Aucun chemin trouvé entre " + start.name + " et " + end.name);
        }
    }

    public void StartComputePathWithSteps()
    {
        StartCoroutine(ComputePathWithStepsCoroutine());
    }

    private IEnumerator ComputePathWithStepsCoroutine()
    {
        PointModel start = gameManager.currentStart;
        PointModel end = gameManager.currentEnd;
        var graph = graphBuilder.graph;
        var distances = new Dictionary<PointModel, float>();
        var previous = new Dictionary<PointModel, PointModel>();
        var toVisit = new List<PointModel>();

        foreach (var city in graph.Keys)
        {
            distances[city] = float.MaxValue;
            toVisit.Add(city);
        }
        distances[start] = 0f;

        while (toVisit.Count > 0)
        {
            yield return new WaitUntil(() => nextStep);
            nextStep = false;

            PointModel current = null;
            float minDist = float.MaxValue;
            foreach (var city in toVisit)
            {
                if (distances[city] < minDist)
                {
                    minDist = distances[city];
                    current = city;
                }
            }
            if (current == null)
                break;
            toVisit.Remove(current);
            if (current == end)
                break;

            foreach (var neighbor in graph[current])
            {
                float newDist = distances[current] + neighbor.Value;
                if (newDist < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = newDist;
                    previous[neighbor.Key] = current;
                }
            }

            if (debugMode)
            {
                Debug.Log("Step: Visiting: " + current.name + " (Distance = " + distances[current] + ")");
                string neighborsStr = "";
                foreach (var n in graph[current].Keys)
                {
                    neighborsStr += n.name + ", ";
                }
                Debug.Log("Neighbors of " + current.name + ": " + neighborsStr);
                string distStr = "";
                foreach (var kvp in distances)
                {
                    distStr += kvp.Key.name + " : " + kvp.Value + ", ";
                }
                Debug.Log("Current distances: " + distStr);
                string prevStr = "";
                foreach (var kvp in previous)
                {
                    prevStr += kvp.Key.name + " ← " + kvp.Value.name + ", ";
                }
                Debug.Log("Previous nodes: " + prevStr);
            }
        }

        List<PointModel> path = new List<PointModel>();
        PointModel stepModel = end;
        while (previous.ContainsKey(stepModel))
        {
            path.Insert(0, stepModel);
            stepModel = previous[stepModel];
        }
        if (stepModel == start)
        {
            path.Insert(0, start);
            string pathStr = "";
            foreach (var pt in path)
            {
                pathStr += pt.name + " → ";
            }
            pathStr = pathStr.TrimEnd(' ', '→');
            Debug.Log("Chemin trouvé : " + pathStr);
            Debug.Log("Coût total : " + distances[end]);

            for (int i = 0; i < path.Count - 1; i++)
            {
                LineModel line = graphBuilder.GetLineBetween(path[i], path[i + 1]);
                if (line != null)
                {
                    line.SetHighlightColor();
                }
                else
                {
                    Debug.LogWarning("Aucune ligne trouvée entre " + path[i].name + " et " + path[i + 1].name);
                }
            }
            foreach (PointModel pt in path)
            {
                if(pt != start && pt != end)
                {
                    pt.SetHighlightColor();
                }
            }
        }
        else
        {
            Debug.LogWarning("Aucun chemin trouvé entre " + start.name + " et " + end.name);
        }
    }

    public void OnNextStepButtonClicked()
    {
        nextStep = true;
    }
}
