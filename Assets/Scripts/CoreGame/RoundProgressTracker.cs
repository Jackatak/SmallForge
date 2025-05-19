using System.Collections.Generic;
using UnityEngine;

public class RoundProgressTracker : MonoBehaviour
{
    [SerializeField] private RecipeManager recipeManager;

    private readonly Dictionary<string, int> submitted = new();

    public void RecordSubmission(ResourceObjectSO metal)
    {
        string id = metal.GetID();
        submitted.TryAdd(id, 0);
        submitted[id]++;
    }


    private void CheckRoundCompletion()
    {
        bool complete = true;

        foreach (var req in recipeManager.CurrentRound.metalsThisRound)
        {
            submitted.TryGetValue(req.metal.GetID(), out int submittedCount);
            if (submittedCount < req.amount)
            {
                complete = false;
                break;
            }
        }

        if (complete)
        {
            Debug.Log("✅ Round Complete!");
            // TODO: Advance round, notify graph, etc.
        }
    }


    public float GetProgressPercent()
    {
        float totalRequired = 0;
        float totalSubmitted = 0;

        foreach (var req in recipeManager.CurrentRound.metalsThisRound)
        {
            totalRequired += req.amount;
            submitted.TryGetValue(req.metal.GetID(), out int submittedCount);
            totalSubmitted += Mathf.Min(submittedCount, req.amount);
        }

        return totalRequired == 0 ? 0 : (totalSubmitted / totalRequired) * 100f;
    }

    
    public int GetSubmittedCount(string id)
    {
        if (submitted.TryGetValue(id, out int count))
        {
            return count;
        }
        return 0;
    }

    
}