using UnityEngine;

public class CrucibleSubmitZone : MonoBehaviour
{
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private RoundProgressTracker progressTracker;

    private void OnTriggerEnter(Collider other)
    {
        WorkshopObject obj = other.GetComponent<WorkshopObject>();
        if (obj == null)
        {
            Debug.Log($"❌ Triggered by non-workshop object: {other.name}");
            return;
        }

        ResourceObjectSO resource = obj.GetResourceData();
        if (resource == null)
        {
            Debug.LogWarning($"⚠️ WorkshopObject has no ResourceObjectSO assigned: {obj.name}");
            return;
        }

        Debug.Log($"🧪 {resource.GetID()} entered crucible.");

        if (!IsMetalInCurrentRound(resource))
        {
            Debug.Log($"⛔ {resource.GetID()} is not part of current round metals.");
            return;
        }

        Debug.Log($"✅ {resource.GetID()} accepted and submitted.");
        progressTracker.RecordSubmission(resource);
        Destroy(obj.gameObject);
    }

    private bool IsMetalInCurrentRound(ResourceObjectSO resource)
    {
        foreach (var metal in recipeManager.CurrentRound.metalsThisRound)
        {
            if (metal.metal.GetID() == resource.GetID())
                return true;
        }
        return false;
    }
}