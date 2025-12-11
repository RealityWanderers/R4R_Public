using TMPro;
using UnityEngine;
using System;
using System.Reflection;

public class FloatValueTextDisplay : MonoBehaviour
{
    public MonoBehaviour targetScript;
    public string fieldName; // Set by custom editor
    public TextMeshProUGUI text;
    public int decimalPlaces = 2;
    public string prefix = "", suffix = "";

    private FieldInfo fieldInfo;
    private PropertyInfo propertyInfo;

    private void Start()
    {
        CacheMemberInfo();
    }

    private void Update()
    {
        if (targetScript == null || text == null || string.IsNullOrEmpty(fieldName)) return;

        float value = 0f;

        if (fieldInfo != null)
        {
            object raw = fieldInfo.GetValue(targetScript);
            value = Convert.ToSingle(raw);
        }
        else if (propertyInfo != null)
        {
            object raw = propertyInfo.GetValue(targetScript);
            value = Convert.ToSingle(raw);
        }

        text.text = $"{prefix}{value.ToString($"F{decimalPlaces}")}{suffix}";
    }

    public void CacheMemberInfo()
    {
        fieldInfo = null;
        propertyInfo = null;

        if (targetScript == null || string.IsNullOrEmpty(fieldName)) return;

        var type = targetScript.GetType();
        fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        propertyInfo = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}