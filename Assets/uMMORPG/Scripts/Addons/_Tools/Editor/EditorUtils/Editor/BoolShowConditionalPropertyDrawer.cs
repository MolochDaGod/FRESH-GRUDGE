#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(BoolShowConditionalAttribute))]
public class BoolShowConditionalPropertyDrawer : BaseShowConditionalPropertyDrawer<BoolShowConditionalAttribute>
{
}
#endif