using System.Collections.Generic;
using UnityEngine;

public class RoundProgressTracker : MonoBehaviour
{
    [SerializeField] private RecipeManager recipeManager;

    private Dictionary<ResourceObjectSO, int> submitted = new Dictionary<ResourceObjectSO, int>();

    public void RecordSubmission(ResourceObjectSO metal)
    {
        if (!submitted.ContainsKey(metal))
            submitted[metal] = 0;

        submitted[metal]++;

        CheckRoundCompletion();
    }

    private void CheckRoundCompletion()
    {
        bool complete = true;

        foreach (var req in recipeManager.CurrentRound.metalsThisRound)
        {
            submitted.TryGetValue(req.metal, out int submittedCount);
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
            submitted.TryGetValue(req.metal, out int submittedCount);
            totalSubmitted += Mathf.Min(submittedCount, req.amount);
        }

        return totalRequired == 0 ? 0 : (totalSubmitted / totalRequired) * 100f;
    }
}