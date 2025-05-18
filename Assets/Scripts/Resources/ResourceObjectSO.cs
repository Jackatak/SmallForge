using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SmallForge/SObj/ResourceObjectSO")]
public class ResourceObjectSO : ScriptableObject
{
    public enum ObjectType
    {
        Resource,
        Scrap,
        Tool
    }
    public string objectName;
    public string objectID;
    public ObjectType objectType; 
    public GameObject prefab;
    public Sprite sprite;
    [Tooltip("Scrap spawn weight (for resources its 1)")]
    [Range(0, 1)]
    public float spawnWeight = 1;
    
    
    public string GetID()
    {
        return objectID;
    }

}
