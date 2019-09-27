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
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Vs17InstallCheck
{
    internal class SinglePrivateRegistryProcessor
    {
        private string filename;
        private int hKey;
        private string instance;
        private string version;

        public SinglePrivateRegistryProcessor(string filename)
        {
            this.filename = filename;
        }

        static (string, string) GetInstanceFromFilename(string filename)
        {
            var dirPart = Path.GetDirectoryName(filename);
            var lastPart = new DirectoryInfo(dirPart).Name;
            var pattern = @"([0-9]+\.[0-9A-Z]+)_([a-f0-9]+)";
            var match = Regex.Match(lastPart, pattern);
            if (match.Success && match.Groups.Count > 2)
            {
                return (match.Groups[1].Value, match.Groups[2].Value);
            }
            return ("", "");
        }

        internal void PrintTestExtensions(string version, string instance)
        {
            const string extKeyName = "Extensions";
            var keyPath = $@"Software\Microsoft\VisualStudio\{version}_{instance}_Config\EnterpriseTools\QualityTools\TestTypes";

            using (var safeRegistryHandle = new SafeRegistryHandle(new IntPtr(hKey), true))
            using (var appKey = RegistryKey.FromHandle(safeRegistryHandle))
            using (var extensionsKey = appKey.OpenSubKey(keyPath, true))
            {
                if (extensionsKey == null)
                {
                    throw new VSPrivateRegException($@"{keyPath} not found in {filename}.");
                }

                var subkeyNames = extensionsKey.GetSubKeyNames();
                foreach (var subkey in subkeyNames)
                {
                    Console.WriteLine($"        {subkey}");
                    using (var subKeyHandle = extensionsKey.OpenSubKey(subkey))
                    using (var extKey = subKeyHandle.OpenSubKey(extKeyName))
                    {
                        if (extKey == null)
                        {
                            throw new VSPrivateRegException($@"subkey {extKeyName} not found under {keyPath}\\{subkey} in {filename}.");
                        }

                        var valueNames = extKey.GetValueNames();
                        foreach (var name in valueNames)
                        {
                            var value = extKey.GetValue(name);
                            Console.WriteLine($"            {name} 0x{value:x}");
                        }
                    }
                }
            }
        }

        private void PrintVSExtensions(string instance)
        {
            var keyPath = $@"Software\Microsoft\VisualStudio\{instance}\ExtensionManager\EnabledExtensions";
            using (var safeRegistryHandle = new SafeRegistryHandle(new IntPtr(hKey), true))
            using (var appKey = RegistryKey.FromHandle(safeRegistryHandle))
            using (var extensionsKey = appKey.OpenSubKey(keyPath, true))
            {
                var valueNames = extensionsKey.GetValueNames();
                foreach (var name in valueNames)
                {
                    var value = extensionsKey.GetValue(name);
                }
            }
        }


        internal void Process()
        {
            hKey = RegistryNativeMethods.RegLoadAppKey(filename);
            (version, instance) = GetInstanceFromFilename(filename);
            PrintTestExtensions(version, instance);
            // PrintVSExtensions(instance);
        }

    }
}