using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGen : BaseContraption
{
    // custom class to hold resource and chance
    [System.Serializable]
    public class ResourceChancePair
    {
        public ResourceObjectSO resource;
        [Range(0f, 1f)]
        public float chance;
    }
    
    [SerializeField] private ResourceChancePair[] resourceChances;
    
    
    [SerializeField] private Convayor nextConvayor;
    
    public static float moveSpeed = 1.0f; // Global movement time
    public static bool stop = false; // Global stop variable

    private bool isMoving = false;
    
    public override void Interact(Player player)
    {
        if (!player.HasWorkshopObject())
        {
            // player is not holding an object
            //Transform resourceObjectTransform = Instantiate(resourceObject.prefab);
            //resourceObjectTransform.GetComponent<WorkshopObject>().SetWorkshopObjectParent(this);
        }
    }
    
    private void Start()
    {
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

        // Try push next
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
}
