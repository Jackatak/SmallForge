using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceVisuals : MonoBehaviour
{
    [SerializeField] private WorkshopObject workshopObject;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Start()
    {
        Player.Instance.OnSelectedContraptionChanged += Player_OnSelectedContraptionChanged;
    }

    private void Player_OnSelectedContraptionChanged(object sender, Player.OnSelectContraptionChangeEventArgs e)
    {
        // Show visuals if this workshop object is currently selected
        if (e.SelectedContraption is FreeWorkshopObject free &&
            free.GetWorkshopObject() == workshopObject)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (GameObject visual in visualGameObjectArray)
        {
            visual.SetActive(true);
        }
    }

    private void Hide()
    {
        if (!this || !gameObject || gameObject.Equals(null)) return;
        gameObject.SetActive(false);
    }
}