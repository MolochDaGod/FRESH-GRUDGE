#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class MToolsSetupClassic3D : MToolsSetupBase
{
    [MenuItem("ManagementTools/Setup/3D/Install: Classic")]
    static void Init()
    {
        // do not open existing window again
        if (HasOpenInstances<MToolsSetupBase>())
            return;

        if (MToolsSetupPage.Classic3DAll.Length == 13)
        {
            // make a new window
            MToolsSetupClassic3D window = (MToolsSetupClassic3D)GetWindowWithRect(typeof(MToolsSetupClassic3D), new Rect(0, 0, 470, 210));
            window.Show();
            
            window.GetPage(MToolsSetupPage.Classic3DAll[0], false);

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

    public override string DefineSymbol => "UMMORPG_3D_C";
    public override float GetProgress() => (float)(index - 1) / 10;

    public override void GetPage(MToolsSetupPage page, bool changeProgress = true)
    {
        base.GetPage(page, changeProgress);

        if (index == 10)
        {
            buttonName = "Finish";
        }

        if (index == 11) // finish
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
                    RevertInstallation(MToolsSetupPage.Classic3DAll[9]);        // Page 9
                    break;
                case 2:
                    Next(MToolsSetupPage.Classic3DAll[8]);                      // Page 8
                    break;
                case 3:
                    Next(MToolsSetupPage.Classic3DAll[7]);                      // Page 7
                    break;
                case 4:
                    Next(MToolsSetupPage.Classic3DAll[6]);                      // Page 6
                    break;
                case 5:
                    Next(MToolsSetupPage.Classic3DAll[5]);                      // Page 5
                    break;
                case 6:
                    Next(MToolsSetupPage.Classic3DAll[4]);                      // Page 4
                    break;
                case 7:
                    Next(MToolsSetupPage.Classic3DAll[3]);                      // Page 3
                    break;
                case 8:
                    Next(MToolsSetupPage.Classic3DAll[2]);                      // Page 2
                    break;
                case 9:
                    Next(MToolsSetupPage.Classic3DAll[1]);                      // Page 1
                    break;
                case 10:
                    Next(MToolsSetupPage.Classic3DAll[12]);                     // Page 12 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Classic3DAll[10], true);          // Page 10
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 1:
                    Next(MToolsSetupPage.Classic3DAll[2]);                      // Page 2
                    break;
                case 2:
                    Next(MToolsSetupPage.Classic3DAll[3]);                      // Page 3                
                    break;
                case 3:
                    Next(MToolsSetupPage.Classic3DAll[4]);                      // Page 4
                    break;
                case 4:
                    Next(MToolsSetupPage.Classic3DAll[5]);                      // Page 5
                    break;
                case 5:
                    Next(MToolsSetupPage.Classic3DAll[6]);                      // Page 6
                    break;
                case 6:
                    Next(MToolsSetupPage.Classic3DAll[7]);                      // Page 7
                    break;
                case 7:
                    Next(MToolsSetupPage.Classic3DAll[8]);                      // Page 8
                    break;
                case 8:
                    Next(MToolsSetupPage.Classic3DAll[9]);                      // Page 9
                    break;
                case 9:
                    Next(MToolsSetupPage.Classic3DAll[10], true, false);        // Page 10
                    break;
                case 10:
                    FinishInstallation(MToolsSetupPage.Classic3DAll[11]);       // Page 11 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Classic3DAll[1]);                 // Page 1                    
                    break;
            }
        }
    }

    public override void DisableDefaultUI(bool enabled) { }
}
#endif