﻿using System.Diagnostics;
using static ArnoldVinkCode.AVFiles;

namespace LibraryShared
{
    public partial class AppLaunchCheck
    {
        public static void Application_UpdateCheck()
        {
            try
            {
                Debug.WriteLine("Checking application update.");

                //Remove old unused files
                File_Delete("Profiles/AppsBlacklistProcess.json");
                File_Delete("Profiles/AppsBlacklistShortcut.json");
                File_Delete("Profiles/AppsBlacklistShortcutUri.json");
                File_Delete("Profiles/AppsCloseLaunchers.json");
                File_Delete("Profiles/AppsCloseTools.json");
                File_Delete("Profiles/ControllersSupported.json");
                File_Delete("Profiles/FileLocations.json");
                File_Delete("Profiles/FpsBlacklistProcess.json");
                File_Delete("Profiles/FpsPositionProcess.json");
                File_Delete("Profiles/ShortcutLocations.json");

                //Rename old file names
                File_Move("Profiles/Apps.json", "Profiles/CtrlApplications.json", true);
                File_Move("Profiles/Controllers.json", "Profiles/DirectControllersProfile.json", true);

                //Check - If updater has been updated
                File_Move("UpdaterNew.exe", "Updater.exe", true);

                //Check - If updater failed to cleanup
                File_Delete("AppUpdate.zip");
            }
            catch { }
        }
    }
}