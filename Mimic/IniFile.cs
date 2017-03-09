using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Create a New INI file to store or load <A class=iAs style="FONT-WEIGHT: normal; FONT-SIZE: 100%; PADDING-BOTTOM: 1px; COLOR: darkgreen; BORDER-BOTTOM: darkgreen 0.07em solid; BACKGROUND-COLOR: transparent; TEXT-DECORATION: underline" href="#" target=_blank itxtdid="3497378">data</A>
/// </summary>
public class IniFile
{
    public string path;
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section,
             string key, string def, StringBuilder retVal,
        int size, string filePath);
    /// <summary>
    /// INIFile Constructor.
    /// </summary>
    /// <PARAM name="INIPath"></PARAM>
    public IniFile(string INIPath)
    {
        path = INIPath;
    }
    /// <summary>
    /// Write Data to the INI File
    /// </summary>
    /// <PARAM name="Section"></PARAM>
    /// Section name
    /// <PARAM name="Key"></PARAM>
    /// Key Name
    /// <PARAM name="Value"></PARAM>
    /// Value Name
    public void IniWriteValue(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, this.path);
    }
    /// <summary>
    /// Read Data Value From the Ini File
    /// </summary>
    /// <PARAM name="Section"></PARAM>
    /// <PARAM name="Key"></PARAM>
    /// <PARAM name="Path"></PARAM>
    /// <returns></returns>
    public string IniReadValue(string Section, string Key)
    {
        StringBuilder temp = new StringBuilder(5000);
        int i = GetPrivateProfileString(Section, Key, "", temp,
                                        5000, this.path);
        return temp.ToString();
    }
}
