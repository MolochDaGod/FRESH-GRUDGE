#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer
{
    MethodInfo _eventMethodInfo = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InspectorButtonAttribute inspectorButtonAttribute = (InspectorButtonAttribute)attribute;
        Rect buttonRect = new Rect(position.x + 15, position.y, label.text.Length * 8, position.height);
        if (GUI.Button(buttonRect, label.text))
        {
            System.Type eventOwnerType = property.serializedObject.targetObject.GetType();
            string eventName = inspectorButtonAttribute.methodName;

            if (_eventMethodInfo == null)
                _eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (_eventMethodInfo != null)
                _eventMethodInfo.Invoke(property.serializedObject.targetObject, null);
            else
                Debug.LogWarning($"InspectorButton: Unable to find method {eventName} in {eventOwnerType}");
        }
    }
}
#endif