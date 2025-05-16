using UnityEditor;
using UnityEngine;

public static partial class GTHF
{
    /// <summary>
    /// Toggles a bool and returns the new value for quick and easy access
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static bool Toggle(this ref bool a)
    {
        a = !a;
        return a;
    }
}
public static partial class GTHF_GUI
{
    /// <summary>
    /// Creates a toggle box in a GUI environment that will change the value when clicked
    /// </summary>
    /// <param name="_val"></param>
    /// <param name="_header"></param>
    public static void GUIToggleBox(this ref bool _val, string _header)
    {
        _val = EditorGUILayout.Toggle(_header, _val);
    }
}