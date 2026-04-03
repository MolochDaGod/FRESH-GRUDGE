#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public sealed class MToolsSetupClassic2D : MToolsSetupBase
{
    [MenuItem("ManagementTools/Setup/2D/Install: Classic")]
    static void Init()
    {
        // do not open existing window again
        if (HasOpenInstances<MToolsSetupBase>())
            return;

        if (MToolsSetupPage.Classic2DAll.Length == 10)
        {
            // make a new window
            MToolsSetupClassic2D window = (MToolsSetupClassic2D)GetWindowWithRect(typeof(MToolsSetupClassic2D), new Rect(0, 0, 470, 210));
            window.Show();
            
            window.GetPage(MToolsSetupPage.Classic2DAll[0], false);

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

    public override string DefineSymbol => "UMMORPG_2D_C";
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
                    RevertInstallation(MToolsSetupPage.Classic2DAll[6]);        // Page 6
                    break;
                case 2:
                    Next(MToolsSetupPage.Classic2DAll[5]);                      // Page 5
                    break;
                case 3:
                    Next(MToolsSetupPage.Classic2DAll[4]);                      // Page 4
                    break;
                case 4:
                    Next(MToolsSetupPage.Classic2DAll[3]);                      // Page 3
                    break;
                case 5:
                    Next(MToolsSetupPage.Classic2DAll[2]);                      // Page 2
                    break;
                case 6:
                    Next(MToolsSetupPage.Classic2DAll[1]);                      // Page 1
                    break;
                case 7:
                    Next(MToolsSetupPage.Classic2DAll[9]);                      // Page 9 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Classic2DAll[7], true);           // Page 7
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 1:
                    Next(MToolsSetupPage.Classic2DAll[2]);                      // Page 2
                    break;
                case 2:
                    Next(MToolsSetupPage.Classic2DAll[3]);                      // Page 3                
                    break;
                case 3:
                    Next(MToolsSetupPage.Classic2DAll[4]);                      // Page 4
                    break;
                case 4:
                    Next(MToolsSetupPage.Classic2DAll[5]);                      // Page 5
                    break;
                case 5:
                    Next(MToolsSetupPage.Classic2DAll[6]);                      // Page 6
                    break;
                case 6:
                    Next(MToolsSetupPage.Classic2DAll[7], true, false);         // Page 7
                    break;
                case 7:
                    FinishInstallation(MToolsSetupPage.Classic2DAll[8]);        // Page 8 (completed)
                    break;
                default:
                    FirstPage(MToolsSetupPage.Classic2DAll[1]);                 // Page 1                    
                    break;
            }
        }
    }

    public override void DisableDefaultUI(bool enabled) { }
}
#endif