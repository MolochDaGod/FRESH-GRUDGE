#if UNITY_EDITOR
using UnityEditor;
[CustomPropertyDrawer(typeof(IntShowConditionalAttribute))]
public class IntShowConditionalPropertyDrawer : BaseShowConditionalPropertyDrawer<IntShowConditionalAttribute>
{
}
#endif