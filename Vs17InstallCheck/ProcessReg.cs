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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Vs17InstallCheck
{
    internal class ProcessReg
    {
        public ProcessReg()
        {
        }

        static string GetInstanceFromFilename(string filename)
        {
            var result = "";
            var dirPart = Path.GetDirectoryName(filename);
            var lastPart = new DirectoryInfo(dirPart).Name;
            var pattern = @"[0-9]+\.[0-9A-Z]+_([a-f0-9]+)";
            var match = Regex.Match(lastPart, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return result;
        }

        internal static void Process(List<string> privateRegistryList)
        {
            var keyPath = $@"Software\Microsoft\VisualStudio";
            foreach (var file in privateRegistryList)
            {
                var hKey = RegistryNativeMethods.RegLoadAppKey(file);
                var instance = GetInstanceFromFilename(file);
                using (var safeRegistryHandle = new SafeRegistryHandle(new IntPtr(hKey), true))
                using (var appKey = RegistryKey.FromHandle(safeRegistryHandle))
                using (var extensionsKey = appKey.OpenSubKey(keypath, true))
                {
                    // get a list of key-value pairs - use the value names to get the values
                    result = extensionsKey == null ? result :
                        extensionsKey.GetValueNames().Select(x => new KeyValuePair<string, string>(x, extensionsKey.GetValue(x).ToString())).ToList();

                    var extensions = extensionsKey?.GetValueNames() ?? Enumerable.Empty<string>();
                    foreach (var key in extensions)
                    {
                        var value = extensionsKey.GetValue(key).ToString();
                        result.Add(new KeyValuePair<string, string>(key, value));
                    }
                }

            }
        }
    }
}