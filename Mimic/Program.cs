﻿using System;
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
            CreateProc(config);
          
            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();

            
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
                    try
                    {
                        if (!ServiceInstaller.ServiceIsInstalled(ServiceName[i]))
                        {
                            ServiceInstaller.InstallAndStart(ServiceName[i], ServiceDispName[i], ServicePath[i], false);
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
        private static void CreateProc(IniFile config)
        {
            bool CreateProc = GetConfigVal("Process", "CreateProc", config);
            string[] ProcName = config.IniReadValue("Process", "CrProcName").Split(';'),
                    ProcArgs = config.IniReadValue("Process", "CrProcArgs").Split(';'),
                    ProcWindowStyle = config.IniReadValue("Process", "CrWindowStyle").Split(';');

            //Console.WriteLine(ProcArgs[0]);
            if(CreateProc && ProcName.Length == ProcArgs.Length)
            {
                Console.WriteLine("======= Create Process with CommandLine =======");
                try
                {
                    for (int i = 0; i < ProcName.Length; i++)
                    {
                        ProcessStartInfo procInfo = new ProcessStartInfo(ProcName[i], ProcArgs[i]);
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
                    try
                    {
                        if (File.Exists(fileName[i]))
                        {
                            File.Delete(fileName[i]);
                            //File.Encrypt(fileName[i]);
                            Console.WriteLine("[+] Successfully deleted " + fileName[i] + ".");
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
                    if (File.Exists(OrigFileName[i]))
                    {
                        File.Move(OrigFileName[i], NewFileName[i]);
                        Console.WriteLine("[+] " + OrigFileName[i] + " renamed to " + NewFileName[i] + "!");
                    }
                    else
                        Console.Write("[-] Error: " + OrigFileName[i] + " does not exist.\n");
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
                    //FileStream file = File.Open(fileName[i], FileMode.Open, FileAccess.ReadWrite);
                    //Console.WriteLine(file.Read();
                    try
                    {
                        using (var file = new FileStream(fileName[i], FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                        {
                            //Console.WriteLine(fileName[i]);
                            //Console.WriteLine(offset);
                            file.Seek(file.Length - Convert.ToInt64(buff.Length), SeekOrigin.Begin);
                            file.Write(buff, 0, buff.Length);
                            file.Close();
                        }
                        Console.WriteLine("[+] Successfully written " + fileName[i] + "!");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("[-] Error: Unauthorized file access.");
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("[-] Error: "+ fileName[i] +" not found.");
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
            string exeTest = File.ReadAllText("Class1.cs");
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
                    CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");

                    System.CodeDom.Compiler.CompilerParameters p = new CompilerParameters();
                    p.GenerateExecutable = true;
                    p.OutputAssembly = fileName[i];
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
                        Console.WriteLine("[+] " + fileName[i] + " created successfully!");
                    }

                    bool runFile = GetConfigVal("File", "CrRunFile", config);                        
                    //Run dropped/created file if runFile == 1
                    if (runFile && ("exe" == fileType[i] || "winexe" == fileType[i]))
                    {
                        try
                        {
                            ProcessStartInfo runExe = new ProcessStartInfo();
                            runExe.FileName = fileName[i];
                            try
                            {
                                runExe.WindowStyle = WindowStyle(WinStyle[i]);
                            }
                            catch
                            {
                                runExe.WindowStyle = ProcessWindowStyle.Normal;
                            }
                            Process.Start(runExe);
                            Console.WriteLine("[+] " + fileName + " started.");
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("[-] Error: " + e.Message + ".");
                        }
                    }
                    else if (runFile && "library" == fileType[i])
                    {
                        var DLL = Assembly.LoadFile(Path.GetFullPath(fileName[i]));
                        Console.WriteLine("[+] " + fileName[i] +  " loaded.");
                        //foreach (Type type in DLL.GetExportedTypes())
                        //{
                        //    var c = Activator.CreateInstance(type);
                        //    type.InvokeMember("Output", BindingFlags.InvokeMethod, null, c, new object[] { @"Hello" });
                        //}
                    }
                }
            }
        }
        private static void DeleteRegistryVal(IniFile config)
        {            
            bool delRegVal = GetConfigVal("Registry", "isDeleteRegVal", config);
            string[] RegNode = config.IniReadValue("Registry", "DelRegKey").Split(';'),
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
    }
}
