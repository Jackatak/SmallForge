using UnityEngine;

namespace InspectorTools
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string ConditionBool;

        public ShowIfAttribute(string conditionBool)
        {
            ConditionBool = conditionBool;
        }
    }
}
