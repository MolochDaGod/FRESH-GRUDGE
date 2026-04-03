
using UnityEditor;
using UnityEngine;

namespace DeepZone.Editor
{
    public class DeepZoneThankYouWindow : EditorWindow
    {
        [MenuItem("Window/DeepZone")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindowWithRect<DeepZoneThankYouWindow>(new Rect(550, 400, 300, 125), true, "DEEPZONE - THANK YOU!", true);
        }

        void OnGUI()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace(); // Center text vertically
            GUIStyle gs = EditorStyles.helpBox;
            gs.richText = true;
            gs.fontSize = 12; 
            GUILayout.Label(("<b>Thank you for your purchase</b>!").ToUpper(), gs);
            GUILayout.FlexibleSpace(); // Center text vertically
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Close", GUILayout.Height(30)))
            {
                PlayerPrefs.SetInt("ShowDeepZoneThankYouWindow", 0);
                this.Close();
            }

            GUILayout.FlexibleSpace();
        }

        [InitializeOnLoadMethod]
        public static void OnImportPackage()
        {
            if (PlayerPrefs.GetInt("ShowDeepZoneThankYouWindow", 1) == 1)
            {
                ShowWindow();
                PlayerPrefs.SetInt("ShowDeepZoneThankYouWindow", 0);
            }
        }

    }
}
