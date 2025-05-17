using UnityEngine;

public class FreeWorkshopObject : BaseContraption
{
    private WorkshopObject workshopObject;

    public FreeWorkshopObject(WorkshopObject obj)
    {
        this.workshopObject = obj;
    }
    public void Initialize(WorkshopObject obj)
    {
        workshopObject = obj;
    }

    public override void Interact(Player player)
    {
        if (!player.HasWorkshopObject() && workshopObject != null)
        {
            workshopObject.SetWorkshopObjectParent(player);
        }
    }

    public override WorkshopObject GetWorkshopObject() => workshopObject;
    public override void SetWorkshopObject(WorkshopObject obj) => workshopObject = obj;
    public override void ClearWorkshopObject() => workshopObject = null;
    public override bool HasWorkshopObject() => workshopObject != null;
    public override Transform GetWorkshopObjectTransform() => workshopObject.transform;
}