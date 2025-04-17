using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class DijkstraManager : MonoBehaviour
{
    public GraphBuilder graphBuilder;
    public GameManager gameManager;

    [Header("Mode Debug & Step-by-Step")]
    public bool nextStep = false;


    public void ComputePath()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        PointModel start = gameManager.currentStart;
        PointModel end = gameManager.currentEnd;
        var graph = graphBuilder.graph;
        if (graph == null || graph.Count == 0)
        {
            UnityEngine.Debug.LogError("Graph is empty or not defined!");
            return;
        }
        if (start == null || end == null)
        {
            UnityEngine.Debug.LogError("Start or end point is not defined!");
            return;
        }
        if (!graph.ContainsKey(start) || !graph.ContainsKey(end))
        {
            UnityEngine.Debug.LogError("Start or end point is not in the graph!");
            return;
        }

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
        stopwatch.Stop(); // Arrête le chrono
        long elapsedMs = stopwatch.ElapsedMilliseconds;

        UnityEngine.Debug.Log("Temps d'exécution de l'algorithme : " + elapsedMs + " ms");
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
            UnityEngine.Debug.Log("Chemin trouvé : " + pathStr);
            UnityEngine.Debug.Log("Coût total : " + distances[end]);

            for (int i = 0; i < path.Count - 1; i++)
            {
                LineModel line = graphBuilder.GetLineBetween(path[i], path[i + 1]);
                if (line != null)
                {
                    line.SetHighlightColor();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Aucune ligne trouvée entre " + path[i].name + " et " + path[i + 1].name);
                }
            }
            foreach (PointModel pt in path)
            {
                if(pt != start && pt != end)
                    pt.SetHighlightColor(); // Change la couleur de surbrillance du point.
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Aucun chemin trouvé entre " + start.name + " et " + end.name);
        }
    }

    public void ComputePathStepByStep()
    {
        StopAllCoroutines();
        StartCoroutine(StepByStepDijkstra());
    }

    IEnumerator StepByStepDijkstra()
    {
        PointModel start = gameManager.currentStart;
        PointModel end = gameManager.currentEnd;
        var graph = graphBuilder.graph;

        if (graph == null || graph.Count == 0 || start == null || end == null)
        {
            UnityEngine.Debug.LogError("Le graphe ou les points de départ/arrivée ne sont pas correctement définis.");
            yield break;
        }

        var distances = new Dictionary<PointModel, float>();
        var previous = new Dictionary<PointModel, PointModel>();
        var toVisit = new List<PointModel>();

        foreach (var city in graph.Keys)
        {
            distances[city] = float.MaxValue;
            toVisit.Add(city);

            // Ne pas changer les couleurs du start ou de l'end
            if (city != start && city != end)
                city.SetNeutralColor();
        }

        distances[start] = 0f;

        while (toVisit.Count > 0)
        {
            yield return new WaitUntil(() => nextStep == true);
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

            if (current != start && current != end)
                current.SetVisitingColor();

            yield return new WaitForSeconds(0.2f);

            if (current == end)
                break;

            toVisit.Remove(current);

            if (current != start && current != end)
                current.SetVisitedColor();

            foreach (var neighbor in graph[current])
            {
                float newDist = distances[current] + neighbor.Value;
                if (newDist < distances[neighbor.Key])
                {
                    distances[neighbor.Key] = newDist;
                    previous[neighbor.Key] = current;

                    var line = graphBuilder.GetLineBetween(current, neighbor.Key);
                    if (line != null)
                        line.SetVisitingColor();
                }
            }
        }

        // Reconstruction du chemin
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
            string pathStr = string.Join(" → ", path.ConvertAll(p => p.name));
            UnityEngine.Debug.Log("Chemin trouvé : " + pathStr);
            UnityEngine.Debug.Log("Coût total : " + distances[end]);

            for (int i = 0; i < path.Count - 1; i++)
            {
                LineModel line = graphBuilder.GetLineBetween(path[i], path[i + 1]);
                if (line != null)
                    line.SetHighlightColor();
            }

            foreach (PointModel pt in path)
            {
                if (pt != start && pt != end)
                    pt.SetHighlightColor();
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("Aucun chemin trouvé entre " + start.name + " et " + end.name);
        }
    }

    public void StopDijkstra()
    {
        StopAllCoroutines();
        nextStep = false;
        foreach (var line in graphBuilder.lines)
        {
            line.SetNeutralColor();
        }
        foreach (var city in graphBuilder.graph.Keys)
        {
            city.SetNeutralColor();
        }
    }

    
    public void OnNextStepButtonClicked()
    {
        nextStep = true;
    }
}
