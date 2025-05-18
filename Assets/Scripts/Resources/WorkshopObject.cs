using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopObject : MonoBehaviour, IWorkshopObjectParent
{
    [SerializeField] private ResourceObjectSO resourceObjectSo;
    private bool canBePickedUp = true;
    
    private IWorkshopObjectParent workshopObjectParent;
    
    public ResourceObjectSO GetResourceObjectSo()
    {
        return resourceObjectSo;
    }
    
    public void SetWorkshopObjectParent(IWorkshopObjectParent newParent)
    {
        if (workshopObjectParent != null && workshopObjectParent != newParent)
        {
            workshopObjectParent.ClearWorkshopObject();
        }

        workshopObjectParent = newParent;

        if (newParent != null)
        {
            if (newParent.HasWorkshopObject())
            {
                Debug.LogWarning("Parent already has a workshop object");
            }

            newParent.SetWorkshopObject(this);
            transform.parent = newParent.GetWorkshopObjectTransform();
            transform.localPosition = Vector3.zero;

            // ✅ Reverse throw effects if picked up by player
            if (newParent is Player)
            {
                // Freeze position again
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints |= RigidbodyConstraints.FreezePositionX;
                    rb.constraints |= RigidbodyConstraints.FreezePositionY;
                    rb.constraints |= RigidbodyConstraints.FreezePositionZ;
                }

                // Reset layer
                gameObject.layer = LayerMask.NameToLayer("Default");

                // Disable collider
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
        else
        {
            transform.parent = null;
        }
    }

    public IWorkshopObjectParent GetWorkshopObjectParent()
    {
        return workshopObjectParent;
    }
    
    public Transform GetWorkshopObjectTransform()
    {
        return transform;
    }

    public void SetWorkshopObject(WorkshopObject workshopObject)
    {
        // No-op: This object is itself a WorkshopObject
    }

    public WorkshopObject GetWorkshopObject()
    {
        return this;
    }

    public void ClearWorkshopObject()
    {
        // No-op or optionally set a flag
    }

    public bool HasWorkshopObject()
    {
        return true;
    }

    public bool CanBePickedUp()
    {
        return canBePickedUp;
    }

    public void SetCanBePickedUp(bool value)
    {
        canBePickedUp = value;
    }
    
    public ResourceObjectSO GetResourceData()
    {
        return resourceObjectSo;
    }

}
