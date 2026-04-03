#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class MToolsSetupRemastered3D : MToolsSetupBase
{
    [MenuItem("ManagementTools/Setup/3D/Install: Remastered")]
    static void Init()
    {
        // do not open existing window again
        if (HasOpenInstances<MToolsSetupBase>())
            return;

        if (MToolsSetupPage.Remastered3DAll.Length == 22)
        {
            // make a new window
            MToolsSetupRemastered3D window = (MToolsSetupRemastered3D)GetWindowWithRect(typeof(MToolsSetupRemastered3D), new Rect(0, 0, 470, 210));
            window.Show();

            window.GetPage(MToolsSetupPage.Remastered3DAll[0], false);

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

    public override string DefineSymbol => "UMMORPG_3D_R";
    public override float GetProgress() => (float)(index - 1) / 19;

    public override void GetPage(MToolsSetupPage page, bool changeProgress = true)
    {
        base.GetPage(page, changeProgress);

        if (index == 19)
        {
            buttonName = "Finish";
        }

        if (index == 20) // finish
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
                AddMToolsSettings(); // add management tools settings and write all unsaved asset changes to disk

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
                    RevertInstallation(MToolsSetupPage.Remastered3DAll[18]);    // Page 18
                    break;
                case 2:
                    Next(MToolsSetupPage.Remastered3DAll[17], false, true);     // Page 17
                    break;
                case 3:
                    Next(MToolsSetupPage.Remastered3DAll[16]);                  // Page 16
                    break;
                case 4:
                    Next(MToolsSetupPage.Remastered3DAll[15]);                  // Page 15
                    break;
                case 5:
                    Next(MToolsSetupPage.Remastered3DAll[14]);                  // Page 14
                    break;
                case 6:
                    Next(MToolsSetupPage.Remastered3DAll[13]);                  // Page 13
                    break;
                case 7:
                    Next(MToolsSetupPage.Remastered3DAll[12]);                  // Page 12
                    break;
                case 8:
                    Next(MToolsSetupPage.Remastered3DAll[11]);                  // Page 11
                    break;
                case 9:
                    Next(MToolsSetupPage.Remastered3DAll[10]);                  // Page 10
                    break;
                case 10:
                    Next(MToolsSetupPage.Remastered3DAll[9]);                   // Page 9
                    break;
                case 11:
                    Next(MToolsSetupPage.Remastered3DAll[8]);                   // Page 8
                    break;
                case 12:
                    Next(MToolsSetupPage.Remastered3DAll[7]);                   // Page 7
                    break;
                case 13:
                    Next(MToolsSetupPage.Remastered3DAll[6]);                   // Page 6
                    break;
                case 14:
                    Next(MToolsSetupPage.Remastered3DAll[5]);                   // Page 5
                    break;
                case 15:
                    Next(MToolsSetupPage.Remastered3DAll[4]);                   // Page 4
                    break;
                case 16:
                    Next(MToolsSetupPage.Remastered3DAll[3]);                   // Page 3
                    break;
                case 17:
                    Next(MToolsSetupPage.Remastered3DAll[2]);                   // Page 2
                    break;
                case 18:
                    Next(MToolsSetupPage.Remastered3DAll[1]);                   // Page 1
                    break;
                case 19:
                    Next(MToolsSetupPage.Remastered3DAll[21]);                  // Page 21 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Remastered3DAll[19], true);       // Page 19
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 1:
                    Next(MToolsSetupPage.Remastered3DAll[2]);                   // Page 2
                    break;
                case 2:
                    Next(MToolsSetupPage.Remastered3DAll[3]);                   // Page 3                
                    break;
                case 3:
                    Next(MToolsSetupPage.Remastered3DAll[4]);                   // Page 4
                    break;
                case 4:
                    Next(MToolsSetupPage.Remastered3DAll[5]);                   // Page 5
                    break;
                case 5:
                    Next(MToolsSetupPage.Remastered3DAll[6]);                   // Page 6
                    break;
                case 6:
                    Next(MToolsSetupPage.Remastered3DAll[7]);                   // Page 7
                    break;
                case 7:
                    Next(MToolsSetupPage.Remastered3DAll[8]);                   // Page 8
                    break;
                case 8:
                    Next(MToolsSetupPage.Remastered3DAll[9]);                   // Page 9
                    break;
                case 9:
                    Next(MToolsSetupPage.Remastered3DAll[10]);                  // Page 10
                    break;
                case 10:
                    Next(MToolsSetupPage.Remastered3DAll[11]);                  // Page 11
                    break;
                case 11:
                    Next(MToolsSetupPage.Remastered3DAll[12]);                  // Page 12
                    break;
                case 12:
                    Next(MToolsSetupPage.Remastered3DAll[13]);                  // Page 13
                    break;
                case 13:
                    Next(MToolsSetupPage.Remastered3DAll[14]);                  // Page 14
                    break;
                case 14:
                    Next(MToolsSetupPage.Remastered3DAll[15]);                  // Page 15
                    break;
                case 15:
                    Next(MToolsSetupPage.Remastered3DAll[16]);                  // Page 16
                    break;
                case 16:
                    Next(MToolsSetupPage.Remastered3DAll[17]);                  // Page 17
                    break;
                case 17:
                    Next(MToolsSetupPage.Remastered3DAll[18]);                  // Page 18
                    // this must be done in this step to be able to set the layer mask
                    string layer = "Environment";
                    AddLayer(layer);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
                case 18:
                    Next(MToolsSetupPage.Remastered3DAll[19], false, false);    // Page 19
                    break;
                case 19:
                    FinishInstallation(MToolsSetupPage.Remastered3DAll[20]);    // Page 20 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Remastered3DAll[1]);              // Page 1                    
                    break;
            }
        }
    }

    public override void DisableDefaultUI(bool enabled)
    {
        MToolsSetup.AdditionalStep(enabled);
    }

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