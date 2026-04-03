using UnityEngine;

/// <summary>
/// Creates a button in the Inspector window.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Field)]
public class InspectorButtonAttribute : PropertyAttribute
{
    public static float defaultButtonWidth = 80;

    public readonly string methodName;
    public float ButtonWidth { get; set; } = defaultButtonWidth;

    public InspectorButtonAttribute(string methodName)
    {
        this.methodName = methodName;
    }
}
