using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainorGen : BaseContraption
{
    [SerializeField] private ResourceObjectSO resourceObjectSo;
    
    public override void Interact(Player player)
    {
        if (!player.HasWorkshopObject())
        {
            // player is not holding an object
            Transform resourceObjectTransform = Instantiate(resourceObjectSo.prefab);
            resourceObjectTransform.GetComponent<WorkshopObject>().SetWorkshopObjectParent(player);
        }

        
    }
    
}
