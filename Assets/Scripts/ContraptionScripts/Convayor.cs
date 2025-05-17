using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Convayor : BaseContraption
{
    [SerializeField] private Convayor nextConvayor;
    
    public static float moveSpeed = 1.0f; // Global movement time
    public static bool stop = false; // Global stop variable

    private bool isMoving = false; 
    
    public override void Interact(Player player)
    {
        if (!HasWorkshopObject())
        {
            //there is no object in the contraption
            if (player.HasWorkshopObject())
            {
                // Player has an object
                player.GetWorkshopObject().SetWorkshopObjectParent(this);
            }
            else
            {
                // player has no object
            }
        }
        else
        {
            //There is an object in the contraption
            if (player.HasWorkshopObject())
            {
                // Player has an object
            }
            else
            {
                // player has no object
                GetWorkshopObject().SetWorkshopObjectParent(player);
            }
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
