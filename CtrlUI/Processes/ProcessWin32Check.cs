﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static ArnoldVinkCode.ProcessClasses;
using static ArnoldVinkCode.ProcessFunctions;
using static ArnoldVinkCode.ProcessWin32Functions;
using static CtrlUI.ImageFunctions;
using static LibraryShared.Classes;

namespace CtrlUI
{
    partial class WindowMain
    {
        //Check if process has multiple processes
        async Task<ProcessMulti> CheckProcessMultiWin32(DataBindApp LaunchApp)
        {
            try
            {
                List<DataBindString> multiAnswers = new List<DataBindString>();
                Process[] multiVariables = GetProcessesByNameOrTitle(Path.GetFileNameWithoutExtension(LaunchApp.PathExe), false, true);
                if (multiVariables.Any())
                {
                    if (multiVariables.Count() > 1)
                    {
                        foreach (Process multiProcess in multiVariables)
                        {
                            try
                            {
                                //Get the process title
                                string ProcessTitle = GetWindowTitleFromProcess(multiProcess);
                                if (ProcessTitle == "Unknown") { ProcessTitle += " (Hidden)"; }
                                if (multiAnswers.Where(x => x.Name.ToLower() == ProcessTitle.ToLower()).Any()) { ProcessTitle += " (" + multiAnswers.Count + ")"; }

                                DataBindString AnswerApp = new DataBindString();
                                AnswerApp.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/App.png" }, IntPtr.Zero, -1);
                                AnswerApp.Name = ProcessTitle;
                                multiAnswers.Add(AnswerApp);
                            }
                            catch { }
                        }

                        DataBindString Answer1 = new DataBindString();
                        Answer1.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/App.png" }, IntPtr.Zero, -1);
                        Answer1.Name = "Launch new instance";
                        multiAnswers.Add(Answer1);

                        DataBindString Answer2 = new DataBindString();
                        Answer2.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/Closing.png" }, IntPtr.Zero, -1);
                        Answer2.Name = "Close all the instances";
                        multiAnswers.Add(Answer2);

                        DataBindString cancelString = new DataBindString();
                        cancelString.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/Close.png" }, IntPtr.Zero, -1);
                        cancelString.Name = "Cancel";
                        multiAnswers.Add(cancelString);

                        DataBindString Result = await Popup_Show_MessageBox(LaunchApp.Name + " has multiple running instances", "", "Please select the instance that you wish to interact with:", multiAnswers);
                        if (Result != null)
                        {
                            if (Result == Answer2)
                            {
                                ProcessMulti processMultiNew = new ProcessMulti();
                                processMultiNew.Type = ProcessType.Win32;
                                processMultiNew.Status = "CloseAll";
                                return processMultiNew;
                            }
                            else if (Result == cancelString)
                            {
                                ProcessMulti processMultiNew = new ProcessMulti();
                                processMultiNew.Type = ProcessType.Win32;
                                processMultiNew.Status = "Cancel";
                                return processMultiNew;
                            }
                            else
                            {
                                return ConvertProcessToProcessMulti(ProcessType.Win32, multiVariables[multiAnswers.IndexOf(Result)]);
                            }
                        }
                        else
                        {
                            ProcessMulti processMultiNew = new ProcessMulti();
                            processMultiNew.Type = ProcessType.Win32;
                            processMultiNew.Status = "Cancel";
                            return processMultiNew;
                        }
                    }
                    else
                    {
                        return ConvertProcessToProcessMulti(ProcessType.Win32, multiVariables.FirstOrDefault());
                    }
                }
            }
            catch { }
            return null;
        }

        //Check process status before launching (True = Continue)
        async Task<bool> CheckLaunchProcessWin32(ProcessMulti processMulti, DataBindApp launchApp)
        {
            try
            {
                Debug.WriteLine("Checking launch process Win32: " + launchApp.Name + " / " + launchApp.ProcessId + " / " + launchApp.WindowHandle);

                //Check the multi process results
                if (processMulti.Status == null) { return true; }
                if (processMulti.Status == "Cancel") { return false; }
                if (processMulti.Status == "CloseAll")
                {
                    //Close all processes Win32
                    CloseAllProcessesWin32ByDataBindApp(launchApp);
                    return false;
                }

                //Focus or Close when process is already running
                List<DataBindString> Answers = new List<DataBindString>();
                DataBindString Answer1 = new DataBindString();
                Answer1.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/MiniMaxi.png" }, IntPtr.Zero, -1);
                Answer1.Name = "Show application";
                Answers.Add(Answer1);

                DataBindString Answer2 = new DataBindString();
                Answer2.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/Closing.png" }, IntPtr.Zero, -1);
                Answer2.Name = "Close application";
                Answers.Add(Answer2);

                DataBindString Answer3 = new DataBindString();
                Answer3.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/Switch.png" }, IntPtr.Zero, -1);
                Answer3.Name = "Restart application";
                Answers.Add(Answer3);

                DataBindString Answer4 = new DataBindString();
                Answer4.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/App.png" }, IntPtr.Zero, -1);
                Answer4.Name = "Launch new instance";
                Answers.Add(Answer4);

                DataBindString cancelString = new DataBindString();
                cancelString.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/Close.png" }, IntPtr.Zero, -1);
                cancelString.Name = "Cancel";
                Answers.Add(cancelString);

                //Check application runtime
                string ApplicationRuntime = string.Empty;
                if (launchApp.Category == AppCategory.Shortcut)
                {
                    ApplicationRuntime = ApplicationRuntimeString(ProcessRuntimeMinutes(GetProcessById(launchApp.ProcessId)), "shortcut process");
                }
                else
                {
                    ApplicationRuntime = ApplicationRuntimeString(launchApp.RunningTime, "application");
                }

                //Show the messagebox
                DataBindString Result = await Popup_Show_MessageBox("What would you like to do with " + launchApp.Name + "?", ApplicationRuntime, "", Answers);
                if (Result != null)
                {
                    if (Result == Answer1)
                    {
                        //Check if process has multiple windows
                        IntPtr processWindowHandle = await CheckProcessWindowsWin32AndWin32Store(launchApp.Name, processMulti);
                        if (processWindowHandle != IntPtr.Zero)
                        {
                            //Minimize the CtrlUI window
                            if (ConfigurationManager.AppSettings["MinimizeAppOnShow"] == "True") { await AppMinimize(true); }

                            //Force focus on the app
                            FocusProcessWindowPrepare(launchApp.Name, processMulti.ProcessId, processWindowHandle, 0, false, false, false);

                            //Launch the keyboard controller
                            if (launchApp.LaunchKeyboard)
                            {
                                LaunchKeyboardController(true);
                            }
                        }
                        else
                        {
                            Popup_Show_Status("Close", "Application has no window");
                            Debug.WriteLine("Show application has no window.");
                        }
                        return false;
                    }
                    else if (Result == Answer2)
                    {
                        Popup_Show_Status("Closing", "Closing " + launchApp.Name);
                        Debug.WriteLine("Closing Win32 application: " + launchApp.Name);

                        //Close the process
                        bool ClosedProcess = false;
                        if (processMulti != null)
                        {
                            ClosedProcess = CloseProcessById(processMulti.ProcessId);
                        }
                        else
                        {
                            ClosedProcess = CloseProcessesByNameOrTitle(Path.GetFileNameWithoutExtension(launchApp.PathExe), false);
                        }

                        //Check if process closed
                        if (ClosedProcess)
                        {
                            //Updating running status
                            launchApp.StatusRunning = Visibility.Collapsed;
                            launchApp.StatusSuspended = Visibility.Collapsed;
                            launchApp.RunningTimeLastUpdate = 0;

                            //Update process count
                            launchApp.ProcessRunningCount = string.Empty;
                        }
                        else
                        {
                            Popup_Show_Status("Closing", "Failed to close the app");
                            Debug.WriteLine("Failed to close the application.");
                        }

                        return false;
                    }
                    else if (Result == Answer3)
                    {
                        Popup_Show_Status("Switch", "Restarting " + launchApp.Name);
                        Debug.WriteLine("Restarting Win32 application: " + launchApp.Name + " / " + processMulti.ProcessId + " / " + processMulti.WindowHandle);

                        string LaunchArgument = launchApp.Argument;
                        if (launchApp.Category == AppCategory.Emulator)
                        {
                            if (string.IsNullOrWhiteSpace(launchApp.RomPath))
                            {
                                LaunchArgument = string.Empty;
                            }
                            else
                            {
                                LaunchArgument += launchApp.RomPath;
                            }
                        }

                        await RestartProcessWin32(processMulti.ProcessId, launchApp.PathExe, launchApp.PathLaunch, LaunchArgument);
                        return false;
                    }
                    else if (Result == Answer4)
                    {
                        Debug.WriteLine("Running new application instance.");
                        return true;
                    }
                    else if (Result == cancelString)
                    {
                        Debug.WriteLine("Cancelling the process action.");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Popup_Show_Status("Close", "Failed showing or closing application");
                Debug.WriteLine("Failed closing or showing the application: " + ex.Message);
            }
            return true;
        }

        //Check if process has multiple windows
        async Task<IntPtr> CheckProcessWindowsWin32AndWin32Store(string appTitle, ProcessMulti processTarget)
        {
            try
            {
                if (processTarget.ProcessThreads.Count > 1)
                {
                    Debug.WriteLine("Found window threads: " + processTarget.ProcessThreads.Count);

                    List<DataBindString> multiAnswers = new List<DataBindString>();
                    List<IntPtr> multiVariables = new List<IntPtr>();
                    foreach (ProcessThread ThreadProcess in processTarget.ProcessThreads)
                    {
                        foreach (IntPtr ThreadWindowHandle in EnumThreadWindows(ThreadProcess.Id))
                        {
                            try
                            {
                                //Validate the window handle
                                if (ThreadWindowHandle == processTarget.WindowHandle || ValidateWindowHandle(ThreadWindowHandle))
                                {
                                    //Get window title
                                    string ClassNameString = GetWindowTitleFromWindowHandle(ThreadWindowHandle);
                                    if (ThreadWindowHandle == processTarget.WindowHandle) { ClassNameString += " (Main Window)"; }
                                    if (multiAnswers.Where(x => x.Name.ToLower() == ClassNameString.ToLower()).Any()) { ClassNameString += " (" + multiAnswers.Count + ")"; }

                                    DataBindString Answer1 = new DataBindString();
                                    Answer1.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/App.png" }, IntPtr.Zero, -1);
                                    Answer1.Name = ClassNameString;

                                    //Add window to selection
                                    if (ThreadWindowHandle == processTarget.WindowHandle)
                                    {
                                        multiAnswers.Insert(0, Answer1);
                                        multiVariables.Insert(0, ThreadWindowHandle);
                                    }
                                    else
                                    {
                                        multiAnswers.Add(Answer1);
                                        multiVariables.Add(ThreadWindowHandle);
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    //Check if there are multiple answers
                    if (multiVariables.Count == 1)
                    {
                        Debug.WriteLine("There is only one visible window, returning the default window.");
                        return multiVariables.FirstOrDefault();
                    }
                    else if (multiVariables.Count == 0)
                    {
                        return IntPtr.Zero;
                    }

                    DataBindString cancelString = new DataBindString();
                    cancelString.ImageBitmap = FileToBitmapImage(new string[] { "pack://application:,,,/Assets/Icons/Close.png" }, IntPtr.Zero, -1);
                    cancelString.Name = "Cancel";
                    multiAnswers.Add(cancelString);

                    //Ask which window needs to be shown
                    DataBindString Result = await Popup_Show_MessageBox(appTitle + " has multiple windows open", "", "Please select the window that you wish to be shown:", multiAnswers);
                    if (Result != null)
                    {
                        if (Result == cancelString)
                        {
                            return IntPtr.Zero;
                        }
                        else
                        {
                            return multiVariables[multiAnswers.IndexOf(Result)];
                        }
                    }
                    else
                    {
                        return IntPtr.Zero;
                    }
                }
                else
                {
                    Debug.WriteLine("Single thread process.");
                    return processTarget.WindowHandle;
                }
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }
}