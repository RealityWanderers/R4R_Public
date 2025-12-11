using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(FloatValueTextDisplay))]
public class FloatValueTextDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FloatValueTextDisplay display = (FloatValueTextDisplay)target;

        EditorGUI.BeginChangeCheck();

        display.targetScript = (MonoBehaviour)EditorGUILayout.ObjectField("Target Script", display.targetScript, typeof(MonoBehaviour), true);

        if (display.targetScript != null)
        {
            string[] floatFields = display.targetScript.GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m =>
                {
                    if (m.MemberType == MemberTypes.Field)
                    {
                        var field = m as FieldInfo;
                        return field != null && (field.FieldType == typeof(float) || field.FieldType == typeof(int));
                    }
                    else if (m.MemberType == MemberTypes.Property)
                    {
                        var prop = m as PropertyInfo;
                        return prop != null && prop.CanRead && (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(int));
                    }
                    return false;
                })
                .Select(m => m.Name)
                .ToArray();

            int selectedIndex = Mathf.Max(0, Array.IndexOf(floatFields, display.fieldName));
            selectedIndex = EditorGUILayout.Popup("Field Name", selectedIndex, floatFields);
            if (floatFields.Length > 0)
            {
                display.fieldName = floatFields[selectedIndex];
            }
        }

        display.text = (TMPro.TextMeshProUGUI)EditorGUILayout.ObjectField("Text", display.text, typeof(TMPro.TextMeshProUGUI), true);
        display.prefix = EditorGUILayout.TextField("Prefix", display.prefix);
        display.suffix = EditorGUILayout.TextField("Suffix", display.suffix);
        display.decimalPlaces = EditorGUILayout.IntField("Decimal Places", display.decimalPlaces);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            display.CacheMemberInfo();
            EditorUtility.SetDirty(display);
        }
    }
}
