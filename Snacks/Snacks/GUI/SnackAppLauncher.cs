using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;

namespace Snacks
{
    [KSPAddon(KSPAddon.Startup.Flight | KSPAddon.Startup.EveryScene, false)]
    public class SnackAppLauncher : MonoBehaviour
    {
        static protected Texture2D appIcon = null;
        static protected ApplicationLauncherButton appLauncherButton = null;
        static protected SnackAppView snackView = new SnackAppView();

        public void Awake()
        {
            appIcon = GameDatabase.Instance.GetTexture("Snacks/Textures/snacks", false);
            GameEvents.onGUIApplicationLauncherReady.Add(SetupGUI);
        }

        public void OnGUI()
        {
            if (snackView.IsVisible())
                snackView.DrawWindow();
        }

        private void SetupGUI()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                if (appLauncherButton == null)
                    appLauncherButton = ApplicationLauncher.Instance.AddModApplication(ShowGUI, HideGUI, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, appIcon);
            }
            else if (appLauncherButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
        }

        private void ShowGUI()
        {
            snackView.SetVisible(true);
        }

        private void HideGUI()
        {
            snackView.SetVisible(false);
        }
    }
}
