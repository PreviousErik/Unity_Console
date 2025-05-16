using UnityEngine;
using UnityEditor;

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
