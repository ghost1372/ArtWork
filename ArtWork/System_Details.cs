/****************************** ghost1372.github.io ******************************\
*	Module Name:	System_Details.cs
*	Project:		MasterCry
*	Copyright (C) 2017 Mahdi Hosseini, All rights reserved.
*	This software may be modified and distributed under the terms of the MIT license.  See LICENSE file for details.
*
*	Written by Mahdi Hosseini <Mahdidvb72@gmail.com>,  2017, 9, 21, 02:45 ب.ظ
*
***********************************************************************************/

using Microsoft.Win32;
using System;
using System.Configuration;
using System.Linq;
using System.Management;

namespace MasterCry
{
    internal class System_Details
    {
        public static string getOperatingSystemInfo()
        {
            var info = "OS Friendly Name: " + FriendlyName() + Environment.NewLine + "OS UserName: " + Environment.UserName + Environment.NewLine +
               "OS User Domain Name: " + Environment.UserDomainName + Environment.NewLine + "OS Version: " + Environment.OSVersion +
               Environment.NewLine + "OS Is 64Bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
               GetDotnetVersionFromRegistry() + Environment.NewLine + Get45or451FromRegistry();
            return info;
        }

        public static string FriendlyName()
        {
            string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (ProductName != "")
            {
                return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                            (CSDVersion != "" ? " " + CSDVersion : "");
            }
            return "";
        }

        private static string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        private static string GetDotnetVersionFromRegistry()
        {
            var ret = string.Empty;
            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey =
                RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {
                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        string name = (string)versionKey.GetValue("Version", "");
                        string sp = versionKey.GetValue("SP", "").ToString();
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (install == "") //no install info, must be later.
                            ret = ("DotNetFramwok Version: " + versionKeyName + "\t" + name);
                        else
                        {
                            if (sp != "" && install == "1")
                                ret = ("DotNetFramwok Version: " + versionKeyName + "\t" + name + " SP" + sp);
                        }
                        if (name != "")
                            continue;

                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (name != "")
                                sp = subKey.GetValue("SP", "").ToString();
                            install = subKey.GetValue("Install", "").ToString();
                            if (install == "") //no install info, must be later.
                                ret = ("DotNetFramwok Version: " + versionKeyName + "\t" + name);
                            else
                            {
                                if (sp != "" && install == "1")
                                    ret = ("DotNetFramwok Version: " + subKeyName + "\t" + name + " SP" + sp);
                                else if (install == "1")
                                    ret = ("DotNetFramwok Version: " + subKeyName + "\t" + name);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                return "4.6 or later";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2 or later";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1 or later";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5 or later";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        private static string Get45or451FromRegistry()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    return ("DotNetFramwok Version: " + CheckFor45DotVersion((int)ndpKey.GetValue("Release")));
                else
                    return ("DotNetFramwok Version: Version 4.5 or later is not detected.");
            }
        }
    }
}