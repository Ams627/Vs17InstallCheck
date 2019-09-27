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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vs17InstallCheck
{
    class Program
    {
        const string vsDirPattern = "1[56789].[0-9]+_[a-z0-9]";
        const string vsProgDataDirPattern = "[a-z0-9]+";

        private static void Main(string[] args)
        {
            try
            {
                var argset = new HashSet<string>(args);

                bool wantTools = (args.Any() && argset.Contains("-t"));
                bool wantReg = (args.Any() && argset.Contains("-r"));

                Console.WriteLine($"Analysing Visual Studio installations - use -t on the command line to check for tools.");
                Console.WriteLine();
                Console.WriteLine("User local appdata entries for Visual Studio");

                var privateRegistryList = new List<string>();

                var userDirs = Directory.GetDirectories(@"c:\users");
                foreach (var dir in userDirs)
                {
                    var userPath = $@"{dir}\appdata\local\microsoft\visualstudio\";
                    if (Directory.Exists(userPath))
                    {
                        var directories = Directory.GetDirectories(userPath);
                        var nameList = directories.Where(x => Regex.Match(new DirectoryInfo(x).Name, vsDirPattern, RegexOptions.IgnoreCase).Success);
                        foreach (var entry in nameList)
                        {
                            var privateRegPath = Path.Combine(entry, "privateregistry.bin");
                            bool exists = File.Exists(privateRegPath);
                            var reportName = exists ? "privateregistry.bin" : "";
                            Console.WriteLine($"    {entry} {reportName}");
                            if (exists)
                            {
                                privateRegistryList.Add(privateRegPath);
                            }
                        }
                    }
                }

                var vsProgramDataFolder = @"C:\ProgramData\Microsoft\VisualStudio\Packages\_Instances";

                Console.WriteLine();
                Console.WriteLine(@"c:\programdata entries representing VS instances:");
                var dirs = Directory.GetDirectories(vsProgramDataFolder);
                var instanceDirs = dirs.Where(x => Regex.Match(new DirectoryInfo(x).Name, vsProgDataDirPattern, RegexOptions.IgnoreCase).Success);
                foreach (var dir in instanceDirs)
                {
                    Console.WriteLine($"    {dir}");
                }

                Console.WriteLine();
                Console.WriteLine(@"c:\programdata details for VS instances (from the state.json file):");
                foreach (var dir in instanceDirs)
                {
                    var stateFile = Path.Combine(dir, "state.json");
                    Console.WriteLine($"    {stateFile}");
                    if (File.Exists(stateFile))
                    {
                        using (StreamReader reader = File.OpenText(stateFile))
                        {
                            JObject vsJsonInfo = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

                            var topLevelInfoObjects = new[] { "installationPath", "installDate", "updateDate", "layoutPath", "installationVersion" };
                            foreach (var tlObject in topLevelInfoObjects)
                            {
                                var value = vsJsonInfo[tlObject] ?? $"NOT FOUND IN {stateFile}";
                                Console.WriteLine($"        {tlObject} - {value}");
                            }
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine(@"Checking c:\program files (x86)\Microsoft Visual Studio");
                var progDirs = Directory.GetDirectories(@"c:\program files (x86)\Microsoft Visual Studio");
                var dirList = new List<string>();
                foreach (var dir in progDirs)
                {
                    var lastPart = new DirectoryInfo(dir).Name;
                    if (lastPart.All(char.IsDigit))
                    {
                        var year = Convert.ToInt32(lastPart);
                        if (year > 2015 && year < 2140)
                        {
                            dirList.Add(dir);
                        }
                    }
                }

                foreach (var dir in dirList)
                {
                    var subDirs = Directory.GetDirectories(dir);
                    foreach (var subDir in subDirs)
                    {
                        Console.WriteLine($"    {subDir}");
                    }
                }

                // we might not want to find exe tools because it could take a while, so get out if we don't:
                if (wantTools)
                {

                    Console.WriteLine();

                    // the list of tools (executables) we are looking for.
                    var toolList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "cl", "csc", "link", "lib", "csc", "msbuild", "devenv", "dumpbin"
                };
                    Console.WriteLine($"Checking for tools: {string.Join(" ", toolList.Select(x => x + ".exe"))}");

                    var toolsFound = new List<string>();
                    var fileStack = new Stack<string>();
                    foreach (var dir in dirList)
                    {
                        fileStack.Push(dir);
                    }

                    // depth first search over filesystem under the program files directories we found earlier for
                    // visual studio installations:
                    while (fileStack.Any())
                    {
                        var nextDir = fileStack.Pop();
                        var files = Directory.GetFiles(nextDir);
                        foreach (var file in files)
                        {
                            var name = Path.GetFileNameWithoutExtension(file);
                            var ext = Path.GetExtension(file);
                            if (ext == ".exe" && toolList.Contains(name))
                            {
                                toolsFound.Add(file);
                            }
                        }
                        var directories = Directory.GetDirectories(nextDir);
                        foreach (var dir in directories)
                        {
                            fileStack.Push(dir);
                        }
                    }

                    toolsFound.ForEach(x => Console.WriteLine($"    {x}"));
                }
                if (wantReg)
                {
                    ProcessReg.Process(privateRegistryList);
                }
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
