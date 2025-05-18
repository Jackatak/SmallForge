using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Behavior;

public class ResourceGen : BaseContraption
{
    [Header("Convayor Functionality")]
    [SerializeField] private Convayor nextConvayor;
    public static float moveSpeed = 1.0f;
    public static bool stop = false;
    private bool isMoving = false;

    [Header("Resource Generation")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private List<ResourceObjectSO> allResources;
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private bool autoStart = false;

    [SerializeField] private BehaviorGraphAgent graphRunner;
    [SerializeField] private string scrapFlagKey = "scrapOnlyMode";

    private List<ResourceObjectSO> scrapResources;
    private Coroutine productionCoroutine;
    private bool isProducing = false;

    private RoundRecipe CurrentRound => recipeManager.CurrentRound;

    public override void Interact(Player player)
    {
        if (isProducing)
            StopProduction();
        else
            StartProduction();
    }

    private void Start()
    {
        if (autoStart) StartProduction();
        scrapResources = allResources.FindAll(r => r.objectType == ResourceObjectSO.ObjectType.Scrap);
        StartCoroutine(ConveyorLoop());
    }

    private IEnumerator ConveyorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (stop || isMoving || !HasWorkshopObject() || nextConvayor == null || nextConvayor.HasWorkshopObject())
                continue;

            WorkshopObject item = GetWorkshopObject();
            ClearWorkshopObject();

            isMoving = true;
            StartCoroutine(MoveItem(item, nextConvayor));
        }
    }

    private IEnumerator MoveItem(WorkshopObject item, BaseContraption target)
    {
        Transform start = item.transform;
        Vector3 startPos = start.position;
        Vector3 endPos = target.GetWorkshopObjectTransform().position;

        float elapsed = 0f;
        float duration = moveSpeed;

        item.transform.SetParent(null);

        while (elapsed < duration)
        {
            if (stop) yield break;

            elapsed += Time.deltaTime;
            item.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        item.SetWorkshopObjectParent(target);
        isMoving = false;

        if (nextConvayor != null)
            nextConvayor.TryPush();
    }

    public void TryPush()
    {
        if (stop || isMoving || !HasWorkshopObject() || nextConvayor == null || nextConvayor.HasWorkshopObject())
            return;

        WorkshopObject item = GetWorkshopObject();
        ClearWorkshopObject();

        isMoving = true;
        StartCoroutine(MoveItem(item, nextConvayor));
    }

    private void StartProduction()
    {
        isProducing = true;
        productionCoroutine = StartCoroutine(ProduceLoop());
    }

    private void StopProduction()
    {
        isProducing = false;
        if (productionCoroutine != null)
        {
            StopCoroutine(productionCoroutine);
            productionCoroutine = null;
        }
    }

    private IEnumerator ProduceLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);

            GameObject instance;

            bool spawnScrap = graphRunner.GetVariable(scrapFlagKey, out BlackboardVariable _);

            if (spawnScrap)
            {
                instance = Instantiate(SelectScrapPrefab(), spawnPoint.position, Quaternion.identity);
            }
            else
            {
                ResourceObjectSO metal = SelectMetalWeighted();
                instance = Instantiate(
                    (metal != null && metal.prefab != null) ? metal.prefab : SelectScrapPrefab(),
                    spawnPoint.position,
                    Quaternion.identity
                );
            }

            WorkshopObject workshopObject = instance.GetComponent<WorkshopObject>();
            if (workshopObject != null)
            {
                workshopObject.SetWorkshopObjectParent(this);
            }
        }
    }

    private GameObject SelectScrapPrefab()
    {
        List<GameObject> weightedPool = new List<GameObject>();

        foreach (var scrap in scrapResources)
        {
            for (int i = 0; i < scrap.spawnWeight; i++)
            {
                weightedPool.Add(scrap.prefab);
            }
        }

        if (weightedPool.Count == 0)
            return null;

        return weightedPool[Random.Range(0, weightedPool.Count)];
    }

    private ResourceObjectSO SelectMetalWeighted()
    {
        List<ResourceObjectSO> pool = new List<ResourceObjectSO>();

        foreach (var metal in CurrentRound.metalsThisRound)
        {
            for (int i = 0; i < metal.amount; i++)
            {
                pool.Add(metal.metal);
            }
        }

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
    
    public override Transform GetWorkshopObjectTransform()
    {
        return spawnPoint;
    }

}
