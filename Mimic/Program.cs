using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using Microsoft.Win32;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using System.Collections;

namespace Mimic
{
    class Program
    {
        static void Main(string[] args)
        {            
            string curDir = Directory.GetCurrentDirectory();
            IniFile config = new IniFile(curDir + "\\config.ini");
            
            WriteRegistry(config);           
            CreateFile( config);          
            WriteFile(config);            
            RenFile(config);         
            DelFile(config);           
            DeleteRegistryKey(config);         
            DeleteRegistryVal(config);
            CreateMutex(config);
            CreateService(config);
            CreateProcWCmdline(config);
            CreateProcess(config);
            ZwWriteVirtualMemory(config);

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();

            
        }
        private static void ZwWriteVirtualMemory(IniFile config)
        {
            bool isWriteVM = GetConfigVal("API", "isWriteVirtualMemory", config);
            string[] targetProc = config.IniReadValue("API", "targetProcess").Split(';'),
                isCreateThread = config.IniReadValue("API", "isCreateThread").Split(';');
            if (isWriteVM)
            {
                Console.WriteLine("======= Write Virtual Memory =======");
                for (int i = 0; i < targetProc.Length; i++)
                {                  
                    Process[] hProcess = Process.GetProcessesByName(targetProc[i]);
                    try
                    {
                        API.OpenProcess(0x1F0FFF, false, hProcess[0].Id);
                        byte[] buffer = Encoding.Unicode.GetBytes("John Pogi\0");
                        int byteswritten = 0;
                        if (API.WriteProcessMemory((int)hProcess[0].Handle, hProcess[0].MainModule.EntryPointAddress.ToInt32(), buffer, buffer.Length, ref byteswritten))
                        {
                            Console.WriteLine("[+] " + hProcess[0].MainModule.FileName + " written successfully");
                            if (isCreateThread[i] == "1")
                            {
                                uint dwThreadID;
                                IntPtr hThread = API.CreateRemoteThread(hProcess[0].Handle, IntPtr.Zero, 0, hProcess[0].MainModule.EntryPointAddress, IntPtr.Zero, 0, out dwThreadID);
                                //Console.ReadLine();
                                Console.WriteLine("[+] Thread Started. Thread ID: " + dwThreadID);
                                API.TerminateThread(hThread, 0);
                            }
                        }                   
                    
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] Error: " + e.Message);
                    }
                }
            }
        }
        private static void CreateProcess(IniFile config)
        {
            bool CreateProc = GetConfigVal("Process", "isCreateProcess", config);
            string[] fileNames = config.IniReadValue("Process", "CPFileName").Split(';');

            if (CreateProc)
            {
                Console.WriteLine("======= Create Process =======");
                for (int i = 0; i < fileNames.Length; i++)
                {
                    try
                    {
                        ProcessStartInfo procInfo = new ProcessStartInfo();
                        procInfo.FileName = changeToken(fileNames[i]);
                        Process.Start(procInfo);
                        Console.WriteLine("[+] " + procInfo.FileName + " started.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] Error: " + e.Message + ".");
                    }
                }
            }            
        }
        private static void CreateMutex(IniFile config)
        {            
            bool CreateMut = GetConfigVal("API", "CreateMutex", config);
            string[] MutexName = config.IniReadValue("API", "CrMutexName").Split(';');

            if (CreateMut && MutexName != null)
            {
                Console.WriteLine("======= Create Mutex =======");
                for (int i = 0; i < MutexName.Length; i++)
                {
                    Mutex mutex = new Mutex(true, MutexName[i]);

                    if (!mutex.WaitOne(2000))
                    {
                        Console.WriteLine("I dont have the mutex!");
                    }
                    else
                    {
                        Console.WriteLine("Press Enter to Dispose Mutex: " + MutexName[i] + ".");
                        //Console.ReadLine();
                        //mutex.ReleaseMutex();
                    }
                }
            }

            //mutex.Dispose();
        }
        private static void CreateService(IniFile config)
        {
            bool CreateService = GetConfigVal("API", "CreateService", config);
            string[] ServiceDispName = config.IniReadValue("API", "CrServiceDispName").Split(';'),
                     ServiceName = config.IniReadValue("API", "CrServiceName").Split(';'),
                     ServicePath = config.IniReadValue("API", "CrServicePath").Split(';'),
                     StartService = config.IniReadValue("API", "StartService").Split(';');

            if (CreateService && ServiceDispName.Length == ServiceName.Length && ServiceName.Length == ServicePath.Length && ServicePath.Length == StartService.Length)
            {
                Console.WriteLine("======= Create Service =======");
                for (int i = 0; i < ServiceDispName.Length; i++)
                {
                    string path = changeToken(ServicePath[i]);
                    try
                    {
                        if (!ServiceInstaller.ServiceIsInstalled(ServiceName[i]))
                        {
                            ServiceInstaller.InstallAndStart(ServiceName[i], ServiceDispName[i], path, false);
                            Console.WriteLine("[+] Service Installed.");
                        }
                        else
                        {
                            Console.WriteLine("[-] Service Name already existing!");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] Error: " + e.Message + ".");
                    }
                }
            }
         
        }
        private static void CreateProcWCmdline(IniFile config)
        {
            bool CreateProc = GetConfigVal("API", "CreateProcwithCMDLine", config);
            string[] ProcArgs = config.IniReadValue("API", "CrProcArgs").Split(';'),
                    ProcWindowStyle = config.IniReadValue("API", "CrWindowStyle").Split(';');

            //Console.WriteLine(ProcArgs[0]);
            if (CreateProc && ProcWindowStyle.Length == ProcArgs.Length)
            {
                Console.WriteLine("======= Create Process with CommandLine =======");
                try
                {
                    for (int i = 0; i < ProcWindowStyle.Length; i++)
                    {
                        ProcessStartInfo procInfo = new ProcessStartInfo("cmd.exe", ProcArgs[i]);
                        try
                        {
                            procInfo.WindowStyle = WindowStyle(ProcWindowStyle[i]);
                        }
                        catch
                        {
                            procInfo.WindowStyle = ProcessWindowStyle.Normal;
                        }
                        Process proc = Process.Start(procInfo);
                        //proc.WaitForExit();
                        Console.WriteLine("[+] Process created.");                    
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("[-] Error: " + e.Message + ".");
                }
            }
        }
        private static void DelFile(IniFile config)
        {          
            string[] fileName = config.IniReadValue("File", "DelFileName").Split(';');
            bool DelFile = GetConfigVal("File", "DelFile", config);

            if (DelFile && fileName != null)
            {
                Console.WriteLine("======= Delete File =======");
                for (int i = 0; i < fileName.Length; i++) 
                {
                    string pathFile = changeToken(fileName[i]);
                    try
                    {
                        if (File.Exists(pathFile))
                        {
                            File.Delete(pathFile);
                            //File.Encrypt(fileName[i]);
                            Console.WriteLine("[+] Successfully deleted " + pathFile + ".");
                        }
                        else
                            Console.WriteLine("[-] Error: File does not exist.");
                    }
                    catch
                    {
                        Console.WriteLine("[-] Error: Error Path.");    
                    }
                }
            }
            
        }
        private static void RenFile(IniFile config)
        {
            string[] OrigFileName = config.IniReadValue("File", "RenOrigFileName").Split(';'),
                     NewFileName = config.IniReadValue("File", "RenNewFileName").Split(';');
            bool RenFile = GetConfigVal("File", "RenFile", config);

            if (RenFile && NewFileName != null && OrigFileName != null)
            {
                Console.WriteLine("======= Rename File =======");
                for (int i = 0; i < OrigFileName.Length; i++)
                {
                    string srcPathFileName = changeToken(OrigFileName[i]),
                        destPathFileName = changeToken(NewFileName[i]);

                    if (File.Exists(srcPathFileName))
                    {
                        File.Move(srcPathFileName, destPathFileName);
                        Console.WriteLine("[+] " + srcPathFileName + " renamed to " + destPathFileName + "!");
                    }
                    else
                        Console.Write("[-] Error: " + srcPathFileName + " does not exist.\n");
                }
            }
        }
        private static void WriteFile(IniFile config)
        {
            string[] fileName = config.IniReadValue("File", "WrFileName").Split(';');
            bool OpenFile = GetConfigVal("File", "WriteFile", config);

            byte[] buff = Encoding.ASCII.GetBytes("John");
            if (OpenFile && fileName != null)
            {
                Console.WriteLine("======= Write To File =======");
                for (int i = 0; i < fileName.Length; i++)
                {
                    string pathFileName = changeToken(fileName[i]);
                    try
                    {                                              
                        using (var file = new FileStream(pathFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                        {
                            //Console.WriteLine(fileName[i]);
                            //Console.WriteLine(offset);
                            if (file.Length > buff.Length)
                            {
                                file.Seek(file.Length - Convert.ToInt64(buff.Length), SeekOrigin.Begin);
                            }
                            file.Write(buff, 0, buff.Length);
                            file.Close();
                        }
                        Console.WriteLine("[+] Successfully written " + pathFileName + "!");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("[-] Error: Unauthorized file access.");
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("[-] Error: " + pathFileName + " not found.");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("[-] Error: I/O error occurs.");
                    }
                }
            }
        }
        private static void CreateFile(IniFile config)
        {
            string exeTest = @"using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Gwapo ko!"");
            Console.ReadLine();
        }
    }
}";
            bool CreateFile = GetConfigVal("File", "isCreateFile", config);
            string[] fileName = config.IniReadValue("File", "CrFileName").Split(';'),
                     WinStyle = config.IniReadValue("File", "CrFileWindowStyle").Split(';'),
                     fileType = config.IniReadValue("File", "FileType").Split(';');
            //Check if CreateFile is Enabled and File name is not empty
            if (CreateFile && fileName != null)
            {
                Console.WriteLine("======= Create File =======");
                for (int i = 0; i < fileName.Length; i++)
                {
                    string filePathName = changeToken(fileName[i]);
                    if ("library" == fileType[i] || "exe" == fileType[i] || "winexe" == fileType[i])
                    {
                        CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");

                        System.CodeDom.Compiler.CompilerParameters p = new CompilerParameters();
                        p.GenerateExecutable = true;
                        p.OutputAssembly = filePathName;
                        p.CompilerOptions = "/t:" + fileType[i];

                        CompilerResults results = codeProvider.CompileAssemblyFromSource(p, exeTest);
                        if (results.Errors.Count > 0)
                        {
                            foreach (CompilerError CompErr in results.Errors)
                            {
                                Console.WriteLine(CompErr.ErrorText);
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("[+] " + filePathName + " created successfully!");
                        }

                        bool runFile = GetConfigVal("File", "CrRunFile", config);
                        //Run dropped/created file if runFile == 1
                        if (runFile && ("exe" == fileType[i] || "winexe" == fileType[i]))
                        {
                            try
                            {
                                ProcessStartInfo runExe = new ProcessStartInfo();
                                runExe.FileName = filePathName;
                                try
                                {
                                    runExe.WindowStyle = WindowStyle(WinStyle[i]);
                                }
                                catch
                                {
                                    runExe.WindowStyle = ProcessWindowStyle.Normal;
                                }
                                Process.Start(runExe);
                                Console.WriteLine("[+] " + filePathName + " started.");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("[-] Error: " + e.Message + ".");
                            }
                        }
                        else if (runFile && "library" == fileType[i])
                        {
                            var DLL = Assembly.LoadFile(Path.GetFullPath(filePathName));
                            Console.WriteLine("[+] " + filePathName + " loaded.");
                            //foreach (Type type in DLL.GetExportedTypes())
                            //{
                            //    var c = Activator.CreateInstance(type);
                            //    type.InvokeMember("Output", BindingFlags.InvokeMethod, null, c, new object[] { @"Hello" });
                            //}
                        }
                    }
                    else
                    {
                        try
                        {
                            File.Create(filePathName).Close();
                            Console.WriteLine("[+] " + filePathName + " created.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[-] Error: " + e.Message + ".");
                        }
                    }
                }
            }
        }
        private static void DeleteRegistryVal(IniFile config)
        {            
            bool delRegVal = GetConfigVal("Registry", "isDeleteRegVal", config);
            string[] RegNode = config.IniReadValue("Registry", "DelRegNode").Split(';'),
                RegKey = config.IniReadValue("Registry", "DelRegKey").Split(';'),
                RegVal = config.IniReadValue("Registry", "DelRegVal").Split(';');


            if (delRegVal && RegNode.Length == RegKey.Length && RegNode.Length == RegVal.Length)
            {
                Console.WriteLine("======= Delete Registry Value =======");
                for (int i = 0; i < RegNode.Length; i++)
                {
                    RegistryKey node = GetRegNode(RegNode[i]);
                    try
                    {
                        RegistryKey key = node.OpenSubKey(RegKey[i], true);

                        if (null != key)
                        {
                            key.DeleteValue(RegVal[i], true);
                            Console.WriteLine("[+] Registry value deleted successfully.");
                        }
                        else
                        {
                            Console.WriteLine("[-] Error: Error opening Subkey: " + RegKey[i] + ".");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] Error: " + e.Message);
                    }
                }
            }
        }
        private static void DeleteRegistryKey(IniFile config)
        {            
            bool DelRegKey = GetConfigVal("Registry", "isDeleteRegKey", config);
            string[] RegNode = config.IniReadValue("Registry", "NodRegNode").Split(';'),
                    RegPath = config.IniReadValue("Registry", "NodRegKey").Split(';'),
                    RegKey = config.IniReadValue("Registry", "DelKey").Split(';');

            if (DelRegKey && RegNode.Length != 0 && RegPath.Length != 0 && RegNode.Length == RegPath.Length) 
            {
                Console.WriteLine("======= Delete Registry Key =======");
                for (int i = 0; i < RegNode.Length; i++)
                {
                    RegistryKey node = GetRegNode(RegNode[i]);

                    try
                    {
                        RegistryKey key = node.OpenSubKey(RegPath[i], true);

                        if (null != key)
                        {
                            if (null != key.OpenSubKey(RegKey[i]))
                            {
                                    key.DeleteSubKeyTree(RegKey[i], true);
                                    Console.WriteLine("[+] Registry Key and child subkeys deleted successfully.");
                            }
                            else
                            {
                                Console.WriteLine("[-] Error: Error deleting Subkey: " + RegKey[i] + ".");
                            }
                            key.Close();
                        }
                        else 
                        {
                            Console.WriteLine("[-] Error: Error opening Subkey: " + RegKey[i] + "."); 
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] Error: " + e.Message);
                    }
                }
            }

        }
        private static void WriteRegistry(IniFile config)
        {
            bool WriteReg = GetConfigVal("Registry", "isWriteReg", config); ;
            string[] RegName = config.IniReadValue("Registry", "WrRegName").Split(';'),
                   RegValue = config.IniReadValue("Registry", "WrRegValue").Split(';'),
                   RegPath = config.IniReadValue("Registry", "WrRegPath").Split(';'),
                   RegType = config.IniReadValue("Registry", "WrRegDataType").Split(';'),
                   RegNode = config.IniReadValue("Registry", "WrRegNode").Split(';');

            if (WriteReg)
            {
                Console.WriteLine("======= Write Registry =======");
                if (RegName.Length == RegPath.Length && RegName.Length == RegPath.Length && RegType.Length == RegName.Length && RegNode.Length == RegName.Length)
                {
                    for (int i = 0; i < RegName.Length; i++)
                    {                                               
                        try
                        {
                            RegistryValueKind type = GetRegType(RegType[i]);
                            RegistryKey node = GetRegNode(RegNode[i]);
                            RegistryKey RegKey = node.OpenSubKey(RegPath[i], true);                            
                            if (null == RegKey)
                            {
                                //RegKey.Close();
                                node.CreateSubKey(RegPath[i], RegistryKeyPermissionCheck.ReadWriteSubTree);
                                node.Close();
                            }

                            RegKey = node.OpenSubKey(RegPath[i], true);

                            if (RegistryValueKind.MultiString == type) 
                            {
                                string[] multiStr = RegValue[i].Split(',');
                                RegKey.SetValue(RegName[i],multiStr, type);  
                            }
                            else if (RegistryValueKind.DWord == type)
                            {
                                UInt16 dword;
                                try
                                {
                                    UInt16.TryParse(RegValue[i], out dword);
                                    RegKey.SetValue(RegName[i], dword, type);
                                }
                                catch
                                {
                                    Console.WriteLine("[-] Error: DWORD Conversion");
                                    break;
                                }
                                
                            }
                            else
                                RegKey.SetValue(RegName[i], RegValue[i], type);                         
                            
                            //node.Close();
                            RegKey.Close();
                            Console.WriteLine("[+] Write Registry: Success!");
                            
                           
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine("Error: " + e.ToString());
                            Console.WriteLine("[-] Write Registry: Failed!");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.WriteLine("Application must be run as Administrator!");
                            Console.WriteLine("[-] Write Registry: Failed!");
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("[-] Error: " + e.Message);
                            Console.WriteLine("[-] Write Registry: Failed!");
                        }
                    }
                }
                else
                    Console.WriteLine("Missing Parameter!");
            }
        }
        private static RegistryValueKind GetRegType(string typeStr)
        {
            RegistryValueKind type = Microsoft.Win32.RegistryValueKind.String;
            if (typeStr == "REG_MULTI_SZ")
            {
                type = Microsoft.Win32.RegistryValueKind.MultiString;
            }
            else if (typeStr == "REG_DWORD")
            {
                type = Microsoft.Win32.RegistryValueKind.DWord;
            }
            else if (typeStr == "REG_SZ")
            {
                type = Microsoft.Win32.RegistryValueKind.String;
            }
            return type;

        }
        private static RegistryKey GetRegNode(string NodeStr) 
        {
            RegistryKey node = Registry.LocalMachine;

            if (NodeStr == "HKCU") 
            {
                node = Registry.CurrentUser;
            }
            else if (NodeStr == "HKCR")
            {
                node = Registry.ClassesRoot;
            }
            else if (NodeStr == "HKCC")
            {
                node = Registry.CurrentConfig;
            }
            else if (NodeStr == "HKU")
            {
                node = Registry.Users;
            }

            return node;
        }
        private static ProcessWindowStyle WindowStyle(string strStyle)
        {
            ProcessWindowStyle style = ProcessWindowStyle.Normal;

            if (strStyle == "SW_HIDE")
            {
                return ProcessWindowStyle.Hidden;
            }
            else if (strStyle == "SW_MAXIMIZE")
            {
                return ProcessWindowStyle.Maximized;
            }
            else if (strStyle == "SW_MINIMIZE")
            {
                return ProcessWindowStyle.Minimized;
            }
 
            return style;
        }
        private static bool GetConfigVal(string section, string key, IniFile config)
        {
            int val;
            int.TryParse(config.IniReadValue(section, key), out val);
            return Convert.ToBoolean(val);

        }
        private static string changeToken(string str)
        {
            string allappdata = @"C:\Documents and Settings\All Users\Application Data",
                   allprograms = @"C:\Documents and Settings\All Users\Start Menu\Programs",
                   rootdir = @"C:", programdir = @"C:\Program Files", programdirx86 = @"C:\Program Files (x86)",
                   systemdir = @"C:\Windows\System32", systemdirx86 = @"C:\Windows\SysWOW64",
                   regrun = @"Software\Microsoft\Windows\CurrentVersion\Run",
                   regrunx86 = @"Software\Wow6342Node\Microsoft\Windows\CurrentVersion\Run",
                   regmain = @"Software\Microsoft\Internet Explorer\Main",
                   regmainx86 = @"Software\Wow6432Node\Microsoft\Internet Explorer\Main",
                   regexplorer = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                   regexplorerx86 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                   tempdir = @"C:\Windows\Temp", windir = @"C:\Windows", userprofile = @"C:\Users",
                   myappdata = @"C:\Users\" + Environment.UserName + @"\appdata\Roaming",
                   mytemp = @"C:\Users\" + Environment.UserName + @"\AppData\Local\Temp",
                   mystartup = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";

            if (Environment.OSVersion.Platform.ToString() == "Win32NT" && Environment.OSVersion.Version.Major.ToString() == "5" && Environment.OSVersion.Version.Minor.ToString() == "1")
            {
                userprofile = @"C:\Documents and Settings";
                myappdata = @"C:\Documents and Settings\" + Environment.UserName + @"\Application Data";
                mytemp = @"C:\Documents and Settings\" + Environment.UserName + @"\Local Settings\Temp";
                mystartup = @"C:\Documents and Settings\" + Environment.UserName + @"Start Menu\Programs\Startup";
            }
           
            return str.Replace("$allappdata$", allappdata).Replace("$allprograms$", allprograms).Replace("$rootdir$", rootdir)
                .Replace("$programdir$", programdir).Replace("$programdirx86$", programdirx86).Replace("$systemdir$", systemdir)
                .Replace("$systemdir$", systemdir).Replace("$systemdirx86$", systemdirx86).Replace("$regrun$", regrun)
                .Replace("$regrunx86$", regrunx86).Replace("$regmain$", regmain).Replace("$regmainx86$", regmainx86)
                .Replace("$regexplorer$", regexplorer).Replace("$regexplorerx86$", regexplorerx86).Replace("$tempdir$", tempdir)
                .Replace("$windir$", windir).Replace("$userprofile$", userprofile).Replace("$myappdata$", myappdata)
                .Replace("$mytemp$", mytemp).Replace("$mystartup$", mystartup);
        }
    }
}
