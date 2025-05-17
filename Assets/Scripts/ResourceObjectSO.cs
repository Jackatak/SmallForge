using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ResourceObjectSO : ScriptableObject
{
    public enum ObjectType
    {
        Resource,
        Scrap,
        Tool
    }
    public string objectName;
    public ObjectType objectType; 
    public Transform prefab;
    public Sprite sprite;
}
