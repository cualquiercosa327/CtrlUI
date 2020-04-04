﻿using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using static DirectXInput.AppVariables;
using static LibraryShared.Classes;

namespace DirectXInput
{
    partial class WindowMain
    {
        //Read from Json file (Deserialize)
        void JsonLoadList_ControllerProfile()
        {
            try
            {
                //Remove all the current controllers
                vDirectControllersProfile.Clear();

                string JsonFile = File.ReadAllText(@"Profiles\DirectControllersProfile.json");
                ControllerProfile[] JsonList = JsonConvert.DeserializeObject<ControllerProfile[]>(JsonFile);
                foreach (ControllerProfile Controller in JsonList)
                {
                    try
                    {
                        vDirectControllersProfile.Add(Controller);
                    }
                    catch { }
                }

                Debug.WriteLine("Reading Controller Profile Json completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed Reading Json: " + ex.Message);
            }
        }

        //Read Json from profile (Deserialize)
        void JsonLoadProfile<T>(ref T deserializeTarget, string profileName)
        {
            try
            {
                string JsonFile = File.ReadAllText(@"Profiles\" + profileName + ".json");
                deserializeTarget = JsonConvert.DeserializeObject<T>(JsonFile);
                Debug.WriteLine("Reading Json file completed: " + profileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Reading Json file failed: " + profileName + "/" + ex.Message);
            }
        }

        //Save to Json file (Serialize)
        void JsonSaveObject(object serializeObject, string profileName)
        {
            try
            {
                //Json settings
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
                jsonSettings.NullValueHandling = NullValueHandling.Ignore;

                //Json serialize
                string serializedObject = JsonConvert.SerializeObject(serializeObject, jsonSettings);

                //Save to file
                File.WriteAllText(@"Profiles\" + profileName + ".json", serializedObject);
                Debug.WriteLine("Saving Json " + profileName + " completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed saving Json " + profileName + ": " + ex.Message);
            }
        }
    }
}