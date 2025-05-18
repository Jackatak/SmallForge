using UnityEngine;
using Unity.Behavior;


public class BehavioUpdater : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent graphRunner;
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private ResourceGen resourceGen;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        graphRunner.SetVariableValue("roundTimeElapsed", timer);
        graphRunner.SetVariableValue("recipeManager", recipeManager);
        graphRunner.SetVariableValue("resourceGen", resourceGen);
    }

    public void ResetTimer()
    {
        timer = 0f;
    }
}