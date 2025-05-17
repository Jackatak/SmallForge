using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseContraption : MonoBehaviour, IWorkshopObjectParent
{
    [SerializeField] private Transform resourceHoldPoint;
    
    private WorkshopObject workshopObject;

    
    public virtual void Interact(Player player)
    {
        // Base interaction logic
        Debug.LogError("BaseContraption interaction");
    }
    
    
    public virtual Transform GetWorkshopObjectTransform()
    {
        return resourceHoldPoint;
    }
    
    public virtual void SetWorkshopObject(WorkshopObject workshopObject)
    {
        this.workshopObject = workshopObject;
    }
    
    public virtual WorkshopObject GetWorkshopObject()
    {
        return workshopObject;
    }
    
    public virtual void ClearWorkshopObject()
    {
        workshopObject = null;
    }
    
    public virtual bool HasWorkshopObject()
    {
        return workshopObject != null;
    }
}
