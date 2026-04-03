using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyInstaller.Editor
{
    /*internal class UnusedAssetsIssueReporter : MonoBehaviour
    {
        [MenuItem("Tools/Report Issue", false, 300)]
        private static void OpenIssuePage()
        {
            const string URL = "https://mmo-indie.com";
            Application.OpenURL(URL);
        }
    }*/

    public partial class Tools_Editor
    {
        public static bool ArrayContains(string[] defines, string define)
        {
            foreach (string def in defines)
            {
                if (def == define)
                    return true;
            }
            return false;
        }

        // -------------------------------------------------------------------------------
        // RemoveFromArray
        // -------------------------------------------------------------------------------
        public static string[] RemoveFromArray(string[] defines, string define)
        {
            return defines.Where(x => x != define).ToArray();
        }
    }
}
