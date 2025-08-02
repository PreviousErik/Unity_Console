using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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