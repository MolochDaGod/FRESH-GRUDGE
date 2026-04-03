#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(StringShowConditionalAttribute))]
public class StringShowConditionalPropertyDrawer : BaseShowConditionalPropertyDrawer<StringShowConditionalAttribute>
{
}
#endif