using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
/// <summary>
/// API used in Aegis
/// </summary>
public class API
{
    /// <summary>
    /// Open a Process
    /// </summary>
    /// <param name="dwDesiredAccess"></param>
    /// <param name="bInheritHandle"></param>
    /// <param name="dwProcessId"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(
    int dwDesiredAccess,
    bool bInheritHandle,
    int dwProcessId
    );
    /// <summary>
    /// Write Process
    /// </summary>
    /// <param name="hProcess"></param>
    /// <param name="lpBaseAddress"></param>
    /// <param name="lpBuffer"></param>
    /// <param name="nSize"></param>
    /// <param name="lpNumberOfBytesWritten"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, 
                                         int nSize, ref int lpNumberOfBytesWritten);
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string DLLName);
    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes,
                  uint dwStackSize, IntPtr lpStartAddress, // raw Pointer into remote process
                  IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);
    [DllImport("kernel32.dll")]
    public static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);
 
}