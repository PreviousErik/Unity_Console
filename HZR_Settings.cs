using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[FilePath("ProjectSettings/HZR_Settings.HZRAsset", FilePathAttribute.Location.ProjectFolder)]
public class HZR_Settings : ScriptableSingleton<HZR_Settings>
{
    [SerializeField]
    private bool m_InstantUsePreviousInput;
    public bool InstantUsePreviousInput
    {
        get => m_InstantUsePreviousInput;
        set
        {
            if (value == m_InstantUsePreviousInput)
                return;
            m_InstantUsePreviousInput = value;
            Debug.Log("Saved stuff");
            Save(true);
        }
    }

    [SerializeField]
    private bool m_CloseConsoleOnSend;
    public bool CloseConsoleOnSend
    {
        get => m_CloseConsoleOnSend;
        set
        {
            if (value == m_CloseConsoleOnSend)
                return;
            m_CloseConsoleOnSend = value;
            Debug.Log("Saved stuff");
            Save(true);
        }
    }

    [SerializeField]
    private int m_SavedConsolEntries;
    public int SavedConsolEntries
    {

        get => m_SavedConsolEntries;
        set
        {
            if (value == m_SavedConsolEntries)
                return;
            m_SavedConsolEntries = value;
            Debug.Log("Saved stuff");
            Save(true);
        }
    }
}
public class ConsoleSettingsProvider : SettingsProvider
{
    public ConsoleSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : 
        base(path, scopes, keywords)
    {
    }

    bool folded;
    float space;
    int Indentation = 250;


    public override void OnGUI(string searchContext)
    {
        base.OnGUI(searchContext);

        GUILayout.Space(20f);
        //folded = EditorGUILayout.Foldout(folded, "Settings");
        //if (folded) space += Time.deltaTime * 100;

        EditorGUILayout.LabelField("Console Settings");
        EditorGUI.indentLevel++;
        CloseConsole(searchContext);
        InstaUseDrop(searchContext);
        EditorGUI.indentLevel--;

    }
    private bool CheckContext(string searchContext, string[] contexts)
    {

        if (string.IsNullOrEmpty(searchContext)) // If there is nothing searched, then it should not check
            return true;

        foreach (string context in contexts)
        {
            if ( context.Contains(searchContext) )
                return true;
        }
        return false;
    }
    private void CloseConsole(string searchContext)
    {
        string[] contexts = { 
            HZR_Settings.instance.CloseConsoleOnSend.ToString(),
            "Enter", 
            "Console",
        };


        if (CheckContext(searchContext, contexts) == false)
            return;

        bool value = HZR_Settings.instance.CloseConsoleOnSend;
        value = EditorGUILayout.Toggle("Close console on send", value);
        HZR_Settings.instance.CloseConsoleOnSend = value;
    }
    private void InstaUseDrop(string searchContext)
    {
        string[] contexts = { 
            HZR_Settings.instance.InstantUsePreviousInput.ToString(),
            "Dropdown", 
            "Console", 
        };


        if (CheckContext(searchContext, contexts) == false)
            return;

        bool value = HZR_Settings.instance.InstantUsePreviousInput;
        value.GUIToggleBox("Instant use previous input");
        HZR_Settings.instance.InstantUsePreviousInput = value;
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new ConsoleSettingsProvider("Custom Tools/Console Settings", SettingsScope.Project);
    }
}