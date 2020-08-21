﻿using ArnoldVinkCode;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using static CtrlUI.AppVariables;
using static LibraryShared.Settings;

namespace CtrlUI
{
    partial class WindowMain
    {
        //Save - Monitor Application Settings
        void Settings_Save()
        {
            try
            {
                cb_SettingsLaunchFullscreen.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "LaunchFullscreen", cb_SettingsLaunchFullscreen.IsChecked.ToString());
                    if ((bool)cb_SettingsLaunchFullscreen.IsChecked)
                    {
                        cb_SettingsLaunchMinimized.IsChecked = false;
                        Setting_Save(vConfigurationCtrlUI, "LaunchMinimized", cb_SettingsLaunchMinimized.IsChecked.ToString());
                    }
                };

                cb_SettingsLaunchMinimized.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "LaunchMinimized", cb_SettingsLaunchMinimized.IsChecked.ToString());
                    if ((bool)cb_SettingsLaunchMinimized.IsChecked)
                    {
                        cb_SettingsLaunchFullscreen.IsChecked = false;
                        Setting_Save(vConfigurationCtrlUI, "LaunchFullscreen", cb_SettingsLaunchFullscreen.IsChecked.ToString());
                    }
                };

                cb_SettingsCloseMediaScreen.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "CloseMediaScreen", cb_SettingsCloseMediaScreen.IsChecked.ToString()); };
                cb_SettingsShowMediaMain.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "ShowMediaMain", cb_SettingsShowMediaMain.IsChecked.ToString()); };
                cb_SettingsMinimizeAppOnShow.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "MinimizeAppOnShow", cb_SettingsMinimizeAppOnShow.IsChecked.ToString()); };

                cb_SettingsLaunchFpsOverlayer.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "LaunchFpsOverlayer", cb_SettingsLaunchFpsOverlayer.IsChecked.ToString()); };

                cb_SettingsShortcutVolume.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "ShortcutVolume", cb_SettingsShortcutVolume.IsChecked.ToString());
                    UpdateControllerHelp();
                };

                cb_SettingsShowOtherShortcuts.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "ShowOtherShortcuts", cb_SettingsShowOtherShortcuts.IsChecked.ToString());
                };

                cb_SettingsShowOtherProcesses.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "ShowOtherProcesses", cb_SettingsShowOtherProcesses.IsChecked.ToString());
                };

                cb_SettingsHideAppProcesses.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "HideAppProcesses", cb_SettingsHideAppProcesses.IsChecked.ToString());
                };

                cb_SettingsHideBatteryLevel.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "HideBatteryLevel", cb_SettingsHideBatteryLevel.IsChecked.ToString());
                    if ((bool)cb_SettingsHideBatteryLevel.IsChecked)
                    {
                        HideBatteryStatus(true);
                    }
                };

                cb_SettingsHideMouseCursor.Click += async (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "HideMouseCursor", cb_SettingsHideMouseCursor.IsChecked.ToString());
                    if ((bool)cb_SettingsHideMouseCursor.IsChecked)
                    {
                        TaskStart_ShowHideMouseCursor();
                        MouseCursorShow();
                    }
                    else
                    {
                        await AVActions.TaskStopLoop(vTask_ShowHideMouse);
                        MouseCursorShow();
                    }
                };

                cb_SettingsHideControllerHelp.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "HideControllerHelp", cb_SettingsHideControllerHelp.IsChecked.ToString());
                    UpdateControllerHelp();
                };

                cb_SettingsShowHiddenFilesFolders.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "ShowHiddenFilesFolders", cb_SettingsShowHiddenFilesFolders.IsChecked.ToString()); };
                cb_SettingsHideNetworkDrives.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "HideNetworkDrives", cb_SettingsHideNetworkDrives.IsChecked.ToString()); };
                cb_SettingsInterfaceSound.Click += (sender, e) => { Setting_Save(vConfigurationCtrlUI, "InterfaceSound", cb_SettingsInterfaceSound.IsChecked.ToString()); };
                cb_SettingsWindowsStartup.Click += (sender, e) => { ManageShortcutStartup(); };

                slider_SettingsFontSize.ValueChanged += (sender, e) =>
                {
                    textblock_SettingsFontSize.Text = "Adjust the application font size: " + Convert.ToInt32(slider_SettingsFontSize.Value);
                    Setting_Save(vConfigurationCtrlUI, "AppFontSize", Convert.ToInt32(slider_SettingsFontSize.Value).ToString());
                    AdjustApplicationFontSize();
                };

                slider_SettingsDisplayMonitor.ValueChanged += async (sender, e) =>
                {
                    textblock_SettingsDisplayMonitor.Text = "Monitor to display the applications on: " + Convert.ToInt32(slider_SettingsDisplayMonitor.Value);
                    Setting_Save(vConfigurationCtrlUI, "DisplayMonitor", Convert.ToInt32(slider_SettingsDisplayMonitor.Value).ToString());
                    await UpdateWindowPosition(true, false);
                };

                slider_SettingsSoundVolume.ValueChanged += (sender, e) =>
                {
                    textblock_SettingsSoundVolume.Text = "User interface sound volume: " + Convert.ToInt32(slider_SettingsSoundVolume.Value) + "%";
                    Setting_Save(vConfigurationCtrlUI, "InterfaceSoundVolume", Convert.ToInt32(slider_SettingsSoundVolume.Value).ToString());
                };

                //Background Settings
                cb_SettingsVideoBackground.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "VideoBackground", cb_SettingsVideoBackground.IsChecked.ToString());
                    UpdateBackgroundMedia();
                };

                cb_SettingsDesktopBackground.Click += (sender, e) =>
                {
                    Setting_Save(vConfigurationCtrlUI, "DesktopBackground", cb_SettingsDesktopBackground.IsChecked.ToString());
                    UpdateBackgroundMedia();
                };

                slider_SettingsBackgroundBrightness.ValueChanged += (sender, e) =>
                {
                    textblock_SettingsBackgroundBrightness.Text = "Background brightness: " + Convert.ToInt32(slider_SettingsBackgroundBrightness.Value) + "%";
                    Setting_Save(vConfigurationCtrlUI, "BackgroundBrightness", Convert.ToInt32(slider_SettingsBackgroundBrightness.Value).ToString());
                    UpdateBackgroundBrightness();
                };

                slider_SettingsBackgroundPlayVolume.ValueChanged += (sender, e) =>
                {
                    textblock_SettingsBackgroundPlayVolume.Text = "Video playback volume: " + Convert.ToInt32(slider_SettingsBackgroundPlayVolume.Value) + "%";
                    Setting_Save(vConfigurationCtrlUI, "BackgroundPlayVolume", Convert.ToInt32(slider_SettingsBackgroundPlayVolume.Value).ToString());
                    UpdateBackgroundPlayVolume();
                };

                slider_SettingsBackgroundPlaySpeed.ValueChanged += (sender, e) =>
                {
                    textblock_SettingsBackgroundPlaySpeed.Text = "Video playback speed: " + Convert.ToInt32(slider_SettingsBackgroundPlaySpeed.Value) + "%";
                    Setting_Save(vConfigurationCtrlUI, "BackgroundPlaySpeed", Convert.ToInt32(slider_SettingsBackgroundPlaySpeed.Value).ToString());
                    UpdateBackgroundPlaySpeed();
                };

                //Save - Socket Client Port
                txt_SettingsSocketClientPortStart.TextChanged += (sender, e) =>
                {
                    //Color brushes
                    BrushConverter BrushConvert = new BrushConverter();
                    Brush BrushInvalid = BrushConvert.ConvertFromString("#CD1A2B") as Brush;
                    Brush BrushValid = BrushConvert.ConvertFromString("#1DB954") as Brush;

                    if (string.IsNullOrWhiteSpace(txt_SettingsSocketClientPortStart.Text))
                    {
                        txt_SettingsSocketClientPortStart.BorderBrush = BrushInvalid;
                        txt_SettingsSocketClientPortRange.BorderBrush = BrushInvalid;
                        return;
                    }

                    if (Regex.IsMatch(txt_SettingsSocketClientPortStart.Text, "(\\D+)"))
                    {
                        txt_SettingsSocketClientPortStart.BorderBrush = BrushInvalid;
                        txt_SettingsSocketClientPortRange.BorderBrush = BrushInvalid;
                        return;
                    }

                    int NewServerPort = Convert.ToInt32(txt_SettingsSocketClientPortStart.Text);
                    if (NewServerPort < 100 || NewServerPort > 65500)
                    {
                        txt_SettingsSocketClientPortStart.BorderBrush = BrushInvalid;
                        txt_SettingsSocketClientPortRange.BorderBrush = BrushInvalid;
                        return;
                    }

                    txt_SettingsSocketClientPortStart.BorderBrush = BrushValid;
                    txt_SettingsSocketClientPortRange.BorderBrush = BrushValid;
                    txt_SettingsSocketClientPortRange.Text = Convert.ToString(NewServerPort + 2);
                    Setting_Save(vConfigurationCtrlUI, "ServerPort", txt_SettingsSocketClientPortStart.Text);
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save the application settings: " + ex.Message);
            }
        }
    }
}