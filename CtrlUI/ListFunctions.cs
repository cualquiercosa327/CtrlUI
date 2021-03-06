﻿using ArnoldVinkCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static CtrlUI.AppBusyWait;
using static CtrlUI.AppVariables;
using static LibraryShared.Classes;
using static LibraryShared.Enums;
using static LibraryShared.Settings;

namespace CtrlUI
{
    partial class WindowMain
    {
        //Combine all the saved lists to make comparison
        IEnumerable<DataBindApp> CombineAppLists(bool includeShortcuts, bool includeProcesses, bool includeLaunchers)
        {
            try
            {
                IEnumerable<DataBindApp> combinedLists = List_Apps.ToList();
                combinedLists = combinedLists.Concat(List_Games.ToList());
                combinedLists = combinedLists.Concat(List_Emulators.ToList());
                if (includeShortcuts) { combinedLists = combinedLists.Concat(List_Shortcuts.ToList()); }
                if (includeProcesses) { combinedLists = combinedLists.Concat(List_Processes.ToList()); }
                if (includeLaunchers) { combinedLists = combinedLists.Concat(List_Launchers.ToList()); }
                return combinedLists;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed combining application lists: " + ex.Message);
                return null;
            }
        }

        //Refresh the processes and status
        async Task RefreshListProcessesWithWait(bool refreshWait)
        {
            try
            {
                //Check if already refreshing
                if (await WaitForBusyBoolCancel(() => vBusyRefreshingProcesses, refreshWait))
                {
                    Debug.WriteLine("Processes are already refreshing, cancelling.");
                    return;
                }

                //Update the refreshing status
                vBusyRefreshingProcesses = true;

                //Load all the active processes
                Process[] processesList = Process.GetProcesses();

                //Refresh the processes list
                await RefreshListProcesses(processesList);

                //Check app running status
                CheckAppRunningStatus(processesList);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed refreshing processes: " + ex.Message);
            }
            //Update the refreshing status
            vBusyRefreshingProcesses = false;
        }

        //Refresh the list status
        void RefreshListStatus()
        {
            try
            {
                ShowHideEmptyList();
                ListsUpdateCount();
                UpdateSearchResults();
            }
            catch { }
        }

        //Refresh the processes list
        async Task RefreshListProcesses(Process[] processesList)
        {
            try
            {
                //Check if processes list is provided
                if (processesList == null)
                {
                    processesList = Process.GetProcesses();
                }

                //List all the currently running processes
                List<int> activeProcessesId = new List<int>();
                List<IntPtr> activeProcessesWindow = new List<IntPtr>();

                //Get the currently running processes
                IEnumerable<DataBindApp> currentListApps = CombineAppLists(true, false, false).Where(x => x.StatusUrlProtocol == Visibility.Collapsed);

                //Update all the processes
                await ListLoadCheckProcessesUwp(activeProcessesId, activeProcessesWindow, currentListApps, false);
                await ListLoadCheckProcessesWin32(processesList, activeProcessesId, activeProcessesWindow, currentListApps, false);

                //Update the application running count and status
                foreach (DataBindApp dataBindApp in currentListApps)
                {
                    try
                    {
                        //Remove closed processes
                        dataBindApp.ProcessMulti.RemoveAll(x => !activeProcessesId.Contains(x.Identifier));
                        dataBindApp.ProcessMulti.RemoveAll(x => !activeProcessesWindow.Contains(x.WindowHandle));

                        //Check the running count
                        int processCount = dataBindApp.ProcessMulti.Count();
                        if (processCount > 1)
                        {
                            //Remove invalid processes
                            dataBindApp.ProcessMulti.RemoveAll(x => x.WindowHandle == IntPtr.Zero);
                            processCount = dataBindApp.ProcessMulti.Count();
                        }

                        //Update the running count
                        if (processCount > 1)
                        {
                            dataBindApp.RunningProcessCount = Convert.ToString(processCount);
                        }
                        else
                        {
                            dataBindApp.RunningProcessCount = string.Empty;
                        }

                        //Update the running status
                        if (processCount == 0)
                        {
                            dataBindApp.StatusRunning = Visibility.Collapsed;
                            dataBindApp.StatusSuspended = Visibility.Collapsed;
                            dataBindApp.RunningProcessCount = string.Empty;
                            dataBindApp.RunningTimeLastUpdate = 0;
                            dataBindApp.ProcessMulti.Clear();
                        }
                    }
                    catch { }
                }

                //Remove no longer running and invalid processes
                Func<DataBindApp, bool> filterProcessApp = x => x.Category == AppCategory.Process && (!x.ProcessMulti.Any() || x.ProcessMulti.Any(z => !activeProcessesWindow.Contains(z.WindowHandle)) || x.ProcessMulti.Any(z => z.WindowHandle == IntPtr.Zero));
                await ListBoxRemoveAll(lb_Processes, List_Processes, filterProcessApp);
                await ListBoxRemoveAll(lb_Search, List_Search, filterProcessApp);
            }
            catch { }
        }

        //Check for empty lists and hide them
        void ShowHideEmptyList()
        {
            try
            {
                Visibility visibilityGames = List_Games.Any() ? Visibility.Visible : Visibility.Collapsed;
                Visibility visibilityApps = List_Apps.Any() ? Visibility.Visible : Visibility.Collapsed;
                Visibility visibilityEmulators = List_Emulators.Any() ? Visibility.Visible : Visibility.Collapsed;
                Visibility visibilityLaunchers = List_Launchers.Any() ? Visibility.Visible : Visibility.Collapsed;
                Visibility visibilityShortcuts = List_Shortcuts.Any() && Convert.ToBoolean(Setting_Load(vConfigurationCtrlUI, "ShowOtherShortcuts")) ? Visibility.Visible : Visibility.Collapsed;
                Visibility visibilityProcesses = List_Processes.Any() && Convert.ToBoolean(Setting_Load(vConfigurationCtrlUI, "ShowOtherProcesses")) ? Visibility.Visible : Visibility.Collapsed;

                AVActions.ActionDispatcherInvoke(delegate
                {
                    sp_Games.Visibility = visibilityGames;
                    sp_Apps.Visibility = visibilityApps;
                    sp_Emulators.Visibility = visibilityEmulators;
                    sp_Launchers.Visibility = visibilityLaunchers;
                    sp_Shortcuts.Visibility = visibilityShortcuts;
                    sp_Processes.Visibility = visibilityProcesses;
                });
            }
            catch { }
        }

        //Update the app running rime
        void UpdateAppRunningTime()
        {
            try
            {
                bool ApplicationUpdated = false;
                //Debug.WriteLine("Updating the application running time.");
                foreach (DataBindApp dataBindApp in CombineAppLists(false, false, false).Where(x => x.RunningTimeLastUpdate != 0))
                {
                    try
                    {
                        if (dataBindApp.StatusRunning == Visibility.Visible && (Environment.TickCount - dataBindApp.RunningTimeLastUpdate) >= 60000)
                        {
                            ApplicationUpdated = true;
                            dataBindApp.RunningTime++;
                            dataBindApp.RunningTimeLastUpdate = Environment.TickCount;
                            //Debug.WriteLine(UpdateApp.Name + " has been running for one minute, total: " + UpdateApp.RunningTime);
                        }
                    }
                    catch { }
                }

                //Save changes to Json file
                if (ApplicationUpdated)
                {
                    JsonSaveApplications();
                }
            }
            catch { }
        }

        //Update the list items count
        void ListsUpdateCount()
        {
            try
            {
                //Debug.WriteLine("Updating the lists count.");

                string List_Games_Count = List_Games.Count.ToString();
                string List_Apps_Count = List_Apps.Count.ToString();
                string List_Emulators_Count = List_Emulators.Count.ToString();
                string List_Launchers_Count = List_Launchers.Count.ToString();
                string List_Shortcuts_Count = List_Shortcuts.Count.ToString();
                string List_Processes_Count = List_Processes.Count.ToString();

                AVActions.ActionDispatcherInvoke(delegate
                {
                    tb_Games_Count.Text = List_Games_Count;
                    tb_Apps_Count.Text = List_Apps_Count;
                    tb_Emulators_Count.Text = List_Emulators_Count;
                    tb_Launchers_Count.Text = List_Launchers_Count;
                    tb_Shortcuts_Count.Text = List_Shortcuts_Count;
                    tb_Processes_Count.Text = List_Processes_Count;
                });
            }
            catch { }
        }
    }
}