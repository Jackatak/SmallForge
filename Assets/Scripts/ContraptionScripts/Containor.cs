using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Containor : BaseContraption, IWorkshopObjectParent
{
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
}
