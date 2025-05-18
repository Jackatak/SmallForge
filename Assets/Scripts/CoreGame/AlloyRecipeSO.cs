using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SmallForge/SObj/AlloyRecipeSO")]
public class AlloyRecipeSO : ScriptableObject
{
    public List<MetalAmount> recipe = new List<MetalAmount>();
}
