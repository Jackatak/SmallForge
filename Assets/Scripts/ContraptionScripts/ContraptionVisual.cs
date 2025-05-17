using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ContraptionVisual : MonoBehaviour
{
    [SerializeField] private BaseContraption baseContraption;
    [SerializeField] private GameObject[] visualGameObjectArray;
    
    private void Start()
    {
        Player.Instance.OnSelectedContraptionChanged += Player_OnSelectedContraptionChanged;
    }
    
    private void Player_OnSelectedContraptionChanged(object sender, Player.OnSelectContraptionChangeEventArgs e)
    {
        if (e.SelectedContraption == baseContraption)
        {
            Show();
        }
        else
            Hide();
    }

    private void Show()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(true);
        }
    }
    
    private void Hide()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(false);
        }    }
}
