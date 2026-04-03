#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class MToolsSetupRemastered2D : MToolsSetupBase
{
    [MenuItem("ManagementTools/Setup/2D/Install: Remastered")]
    static void Init()
    {
        // do not open existing window again
        if (HasOpenInstances<MToolsSetupBase>())
            return;

        if (MToolsSetupPage.Remastered2DAll.Length == 10)
        {
            // make a new window
            MToolsSetupRemastered2D window = (MToolsSetupRemastered2D)GetWindowWithRect(typeof(MToolsSetupRemastered2D), new Rect(0, 0, 470, 210));
            window.Show();

            window.GetPage(MToolsSetupPage.Remastered2DAll[0], false);

            if (MToolsSettings.Settings != null)
            {
                if (MToolsSettings.Settings.isInstalled)
                {
                    window.description = "Management Tools addon is already installed.";
                    window.showUninstallOption = true;
                    window.showButton = false;
                }
            }
        }
    }

    public override string DefineSymbol => "UMMORPG_2D_R";
    public override float GetProgress() => (float)(index - 1) / 7;

    public override void GetPage(MToolsSetupPage page, bool changeProgress = true)
    {
        base.GetPage(page, changeProgress);

        if (index == 7)
        {
            buttonName = "Finish";
        }

        if (index == 8) // finish
        {
            index = 0;

            if (Uninstallation)
            {
                MToolsSettings asset = MToolsSettings.Settings;
                if (asset != null)
                {
                    asset.isInstalled = false;
                    EditorUtility.SetDirty(asset);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                progressText = "Uninstall Completed";
                showPath = false;
                Uninstallation = false;
            }
            else
            {
                AddMToolsSettings(false); // add management tools settings and write all unsaved asset changes to disk

                progressText = "Completed";
                showUninstallOption = true;
                showButton = false;
                buttonName = "Next";
            }
        }
    }

    public override void SwitchPage()
    {
        if (Uninstallation)
        {
            switch (index)
            {
                case 1:
                    RevertInstallation(MToolsSetupPage.Remastered2DAll[6]);     // Page 6
                    break;
                case 2:
                    Next(MToolsSetupPage.Remastered2DAll[5]);                   // Page 5
                    break;
                case 3:
                    Next(MToolsSetupPage.Remastered2DAll[4]);                   // Page 4
                    break;
                case 4:
                    Next(MToolsSetupPage.Remastered2DAll[3]);                   // Page 3
                    break;
                case 5:
                    Next(MToolsSetupPage.Remastered2DAll[2]);                   // Page 2
                    break;
                case 6:
                    Next(MToolsSetupPage.Remastered2DAll[1]);                   // Page 1
                    break;
                case 7:
                    Next(MToolsSetupPage.Remastered2DAll[9]);                   // Page 9 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Remastered2DAll[7], true);        // Page 7
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 1:
                    Next(MToolsSetupPage.Remastered2DAll[2]);                   // Page 2
                    break;
                case 2:
                    Next(MToolsSetupPage.Remastered2DAll[3]);                   // Page 3                
                    break;
                case 3:
                    Next(MToolsSetupPage.Remastered2DAll[4]);                   // Page 4
                    break;
                case 4:
                    Next(MToolsSetupPage.Remastered2DAll[5]);                   // Page 5
                    break;
                case 5:
                    Next(MToolsSetupPage.Remastered2DAll[6]);                   // Page 6
                    break;
                case 6:
                    Next(MToolsSetupPage.Remastered2DAll[7], true, false);      // Page 7
                    break;
                case 7:
                    FinishInstallation(MToolsSetupPage.Remastered2DAll[8]);     // Page 8 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Remastered2DAll[1]);              // Page 1                    
                    break;
            }
        }
    }

    public override void DisableDefaultUI(bool enabled) { }

    // unused
    bool MoveFolder(string oldPath, string newPath)
    {
        if (string.IsNullOrEmpty(AssetDatabase.ValidateMoveAsset(oldPath, newPath)))
        {
            AssetDatabase.MoveAsset(oldPath, newPath);
            AssetDatabase.Refresh();
            return true;
        }
        //Debug.Log($"The asset is already at '{newPath}'.");
        return false;
    }
}
#endif