// Copyright (c) Adrian Sims 2019
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Vs17InstallCheck
{
    class RegUninstallEntry
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public DateTime InstallDate { get; set; }
        public string InstallLocation { get; set; }
        public string ModifyPath { get; set; }
        public string RepairPath { get; set; }
        public string UninstallString { get; set; }
    }

    static class RegUnInstall
    {
        public static void GetProgEntries(List<RegUninstallEntry> list)
        {
            const string progKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var progkeyHandle = key.OpenSubKey(progKey))
            {
                var subkeynames = progkeyHandle.GetSubKeyNames();
                foreach (var subkeyName in subkeynames)
                {
                    using (var subkeyHandle = progkeyHandle.OpenSubKey(subkeyName))
                    {
                        string displayName = (string)subkeyHandle.GetValue("DisplayName");
                        if (string.IsNullOrEmpty(displayName))
                        {
                            continue;
                        }
                        string installLocation = (string)subkeyHandle.GetValue("InstallLocation");
                        if (string.IsNullOrEmpty(installLocation))
                        {
                            continue;
                        }


                        if (displayName.StartsWith("Visual Studio") || installLocation.Contains("Microsoft Visual Studio"))
                        {
                            var dateString = (string)subkeyHandle.GetValue("InstallDate");
                            if (!DateTime.TryParseExact(dateString, "yyyyMMdd",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var date))
                            {
                                date = DateTime.MinValue;
                            }


                            list.Add(new RegUninstallEntry
                            {
                                Key = subkeyName,
                                DisplayName = displayName,
                                DisplayVersion = (string)subkeyHandle.GetValue("DisplayVersion"),
                                InstallDate = date,
                                InstallLocation = installLocation,
                                ModifyPath = (string)subkeyHandle.GetValue("ModifyPath"),
                                RepairPath = (string)subkeyHandle.GetValue("RepairPath"),
                                UninstallString = (string)subkeyHandle.GetValue("UninstallString"),
                            });
                        }
                    }
                }
            }
        }
    }
}
