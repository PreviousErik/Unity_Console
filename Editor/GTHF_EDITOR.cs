using UnityEditor;
public static partial class GTHF_EDITOR
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
