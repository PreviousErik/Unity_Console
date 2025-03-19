using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace Erik.Systems.Console
{
    /* Knows issues
     * If you add a console command, and then destroy/unload the object/script that made it, the command will still be callable, but will cause errors if called
     * Could remove them if they return null i guess ¯\_(ツ)_/¯
    */
    public abstract class HZR_Console : MonoBehaviour
    {
        private static HZR_Console instance;

        private bool consoleActive;
        public bool ConsoleActive
        {
            get => consoleActive;
            set
            {
                if ( value == consoleActive )
                    return;

                consoleActive = value;
                ConsoleToggleUpdate?.Invoke(value);
            }
        }

        public static Dictionary<string, ConsoleCommand> ConComDict;
        public static Dictionary<string, List<string>> DescriptionDict;

        ConsoleControlls inputActions;

        private static List<string> pastEntries;
        private static List<ConsoleEntry> consoleLog;

        int shownEntry = 0;
        string field;
        bool justMarried;

        GameObject clickedObject;

        #region Subscription

        private event Action<bool> ConsoleToggleUpdate;

        /// <summary>
        /// Subscribe to get the updates on if the console is being open or not
        /// </summary>
        /// <param name="_func"></param>
        public static void SubscribeToTurnOn(Action<bool> _func)
        {
            instance.ConsoleToggleUpdate += _func;
            _func.Invoke(instance.consoleActive); // send the update to keep them in the loop if its already open
        }
        public static void UnsubscribeToTurnOn(Action<bool> _func) => instance.ConsoleToggleUpdate -= _func; 

        #endregion

        #region Console Basics

        protected HZR_Console()
        {
            // has to be set in the constructor as its read only.
            // Safer this way :D
            instance = this;
        }

        private void Awake()
        {
            Debug.Log("Awake call on console");
            EnableConsole();
            /*if (Application.isEditor)
            {
                EnableConsole();
                Debug.Log($"Console enabled since we are in the editor!");
                return;
            }
            string[] args = System.Environment.GetCommandLineArgs(); // gets launchCommandLines
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-Console_Enable")
                { 
                    EnableConsole();
                }
                else
                    Debug.Log($"Console not enabled! \nCode entered: {args[i]}");
            }*/
        }

        private void EnableConsole()
        {
            DontDestroyOnLoad(this);
            gameObject.SetActive(true);
            Init();
        }

        protected virtual void Init()
        {
            inputActions = new ConsoleControlls();
            inputActions.Console.Enable();
            inputActions.Console.OpenConsole.started            += ToggleConsole;
            inputActions.Console.ChoosePreviousInput.started    += DirectionThings;
            inputActions.Console.Enter.started                  += ProcessConsoleEntry;
            inputActions.Console.Mouse.started                  += HandleGettingTargetReference;

            ConComDict = new Dictionary<string, ConsoleCommand>();
            DescriptionDict = new Dictionary<string, List<string>>();

            AddServerCommands();
            AddManipulationCommands();
            AddPlayerCommands();
            AddItemsCommands();
            AddSettingsCommands();
            AddBasicCommands();

            consoleLog = new List<ConsoleEntry>()
            {
                new ConsoleEntry ("Console enabled!",                 Color.green ),
                new ConsoleEntry ("Server status: Not started",       Color.yellow ),
            };
            pastEntries = new List<string>() { "Join 76561198161985973", "Host", "Help", "C_Settings" };
        }

        protected abstract object GetPlayerReference();

        private void OnGUI()
        {
            if (consoleActive == false) return;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height / 3), "");
            GUI.Box(new Rect(0, (Screen.height / 3) - 20, Screen.width, 20), "");
            GUI.SetNextControlName("TextField");
            // Redo with a dropdown and highlights, and if you continue typing without hitting right or similar,
            // then you just continue as it was, perhaps have a grayed out version behind it
            string newText = GUI.TextField(new Rect(0, (Screen.height / 3) - 20, Screen.width, 20), field);
            GUI.FocusControl("TextField");
            if (newText != field)
            {
                field = newText;
                shownEntry = 0;
            }
            if (shownEntry > 0)
            {
                GUI.Box(new Rect(0, Screen.height / 3, Screen.width, (pastEntries.Count * 15) + 5), "");
                GUI.backgroundColor = Color.clear;
                for (int i = pastEntries.Count - 1; i >= 0; i--)
                {
                    GUI.contentColor = i == shownEntry - 1 ? Color.yellow : Color.white;
                    GUI.Label(new Rect(0, (Screen.height / 3) + (15 * i), Screen.width, 20), pastEntries[ i ]);
                }
            }
            GUI.backgroundColor = Color.clear;
            int yPos = (Screen.height / 3) - 40;
            foreach (ConsoleEntry Log in consoleLog)
            {
                GUI.contentColor = Log.textColor;
                GUI.Label(new Rect(0, yPos, Screen.width, 20), Log.text);
                yPos -= 15;
            }
            if (justMarried)
            {
                // move the carret to the end of the textfield
                TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                editor.cursorIndex = field.Length;
                editor.selectIndex = field.Length;
            }
        }

        private void HandleGettingTargetReference(InputAction.CallbackContext context)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject() == true)
                return; // pointer over UI

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                clickedObject = hit.collider.gameObject;
                Log($"Clicked on {clickedObject.name}", Color.green);
            }
        }

        public sealed class ConsoleEntry
        {
            public readonly Color textColor;
            public readonly string text;

            public ConsoleEntry(string text, Color textColor)
            {
                this.textColor = textColor;
                this.text = text;
            }
        }

        private void ChooseThing(InputAction.CallbackContext context)
        {
            if (shownEntry > 0)
            {
                field = pastEntries[ shownEntry -1 ];
                shownEntry = 0;
                justMarried = true;
                return;
            }
        }
        
        private void ToggleConsole(InputAction.CallbackContext context) => ToggleConsole();

        private void ToggleConsole()
        {
            ConsoleActive = !ConsoleActive;
            if (ConsoleActive == true)
                TurnOnConsole();
            else
                TurnOffConsole();
        }

        private void TurnOnConsole()
        {
            Debug.Log($"The console has been enabled!");
            inputActions.Console.ChoosePreviousInput.Enable();
            inputActions.Console.Enter.Enable();
            shownEntry = 0;
            field = string.Empty;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void TurnOffConsole()
        {
            Debug.Log($"The console has been disabled!");
            inputActions.Console.ChoosePreviousInput.Disable();
            inputActions.Console.Enter.Disable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void DirectionThings(InputAction.CallbackContext context)
        {
            shownEntry = Mathf.Clamp(shownEntry - (int)context.ReadValue<float>(), 0, pastEntries.Count);
        }

        #endregion

        #region Logging functionality

        public static void LogError(string message) => Log(message, Color.red);
        
        public static void LogWarning(string message) => Log(message, Color.yellow);
        
        public static void Log(string message) => Log(message, Color.white);

        public static void Log(string message, Color messageColor)
        {
            consoleLog.Insert(0, new ConsoleEntry(message, messageColor));
            if (consoleLog.Count > 20)
                consoleLog.RemoveAt(consoleLog.Count - 1);
        }

        #endregion

        #region Command processing

        private void ProcessConsoleEntry(InputAction.CallbackContext context)
        {
            if (shownEntry > 0) // If something was chosen from the previous dropdown
            {
                ChooseThing(new InputAction.CallbackContext());
                if (HZR_Settings.instance.InstantUsePreviousInput == false)
                    return;
            }

            if (string.IsNullOrEmpty(field) == true)
                return;

            pastEntries.Insert(0, field);
            if (pastEntries.Count > 5)
                pastEntries.RemoveAt(pastEntries.Count - 1);

            ProcessCommand(field.Split(' '));

            field = "";

            if (HZR_Settings.instance.CloseConsoleOnSend)
                ToggleConsole();

        }
        
        private void ProcessCommand(string[] parts)
        {
            if (ConComDict.TryGetValue($"{parts[ 0 ]}|{parts.Length - 1}", out ConsoleCommand command) == false)
            {
                LogWarning("The Command you tried to call, does not exist" );
                Log("Type \"Help\" if you need it");
                return;
            }

            if (command._varTypes.Length != parts.Length - 1)
            {
                LogWarning($"The Command \"{command._commandID}\" requiers " + (command._varTypes.Length < parts.Length ? "fewer" : "more") + " variables");
                Log("Type \"Help\" if you need it");
                return;
            }

            if (ConvertText(parts[ 1.. ], command, out List<object> converted) == false)
            {
                LogWarning("The variables given were not written correctly");
                Log("Type \"Help\" if you need it");
                return;
            }

            command.Execute(converted.ToArray());
        }

        private bool ConvertText(string[] parts, ConsoleCommand command, out List<object> converted)
        {
            converted = new List<object>();
            // make sure to check and add the targets (chosen and player)
            if ( command._usePlayerRef == true )
            {
                Log("Used the player ref");
                converted.Add(GetPlayerReference());
            }
            if (command._useTarget == true )
            {
                Log("Used the target ref");
                converted.Add(clickedObject);
            }
            // TODO: Add a function to click and highlight anything in the scene and use as a reference
            for ( int i = 0; i < parts.Length; i++)
            {
                try
                {
                    converted.Add(Convert.ChangeType(parts[i], command._varTypes[i]));
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Should be called only in the start function or instantiated later on in the game
        /// </summary>
        /// <param name="commands"></param>
        public static void AddCommands(params ConsoleCommand[] commands)
        {
            foreach (ConsoleCommand command in commands)
            {
                AddCommand(command._commandID, command);
            }
        }
        // Can remove the need to send in the ID seperatly, but will keep for now
        // when typing a command, the command should have all the variabels needed after it when its showing what commands you are close to typing
        // and when you are typing one, the variabels needed should also be shown after, as to guide the users in what is missing, could use one colour for needed, and one for optional
        private static void AddCommand(string ID, ConsoleCommand command)
        {
            int varCount = command._varTypes.Length;
            string fullID =  $"{ ID }|{varCount}";
            // Contains the actuall ID, also using the var count, so you can have multiple types using the same start word
            // as of right now, i dont know a good way of making more variants viable, so for example if you want 2 commands with both the same start word and same count of variables.
            // could make it so that it checks the variables, however this could be problematic as there are alot of different variables that could be read the same way
            // i could make it so that it only accepts certain types, so you can only use float, and not ulong, short, or int
            // TODO: look into this later IF NECCESSARY!
            Debug.Log("Added " + fullID);
            if (ConComDict.ContainsKey(fullID)) 
            {
                Debug.LogWarning("The Command you tried to add, allready exists" );
                return;
                
            }

            ConComDict.Add(fullID, command);

            string commandType = command._commandType.ToString();

            string finalDescription = $"Command: {ID}, ";
            for (int i = 0; i < varCount; i++)
            {
                if (i == 0)
                    finalDescription += '[';
                finalDescription += command._varTypes[i].Name;
                if (i + 1 == varCount)
                    finalDescription += ']';
                finalDescription += ", ";
            }
            finalDescription += command._commandDescription;

            // Adds the description and in what category it belonges to
            if (DescriptionDict.ContainsKey(commandType) == false)
                DescriptionDict.Add(commandType, new List<string>() { finalDescription });
            else
                DescriptionDict[ commandType ].Add(finalDescription);
        }

        #endregion

        #region Commands
        protected virtual void AddServerCommands()
        {

        }

        protected virtual void AddManipulationCommands()
        {

        }

        protected virtual void AddPlayerCommands()
        {

        }

        protected virtual void AddItemsCommands()
        {

        }

        protected virtual void AddSettingsCommands()
        {
            AddCommands(
            new ConsoleCommand("C_Settings", "Change the settings on the console", ConsoleCommandType.C_Settings, (object[] ha) =>
            {

            }, typeof(string), typeof(string)));
        }

        protected virtual void AddBasicCommands()
        {
            AddCommands(

            new ConsoleCommand("AddCommands", "Call with code to activate groups of consolecommands", ConsoleCommandType.Basics, (object[] ha) =>
            {
                Log("This function has not been implemented yet");
            },
            typeof(string)),

            new ConsoleCommand("Quit", "Quits the game", ConsoleCommandType.Basics, (object[] ha) =>
            {
                Application.Quit();
            }),

            new ConsoleCommand("Help", "", ConsoleCommandType.Basics, (object[] ha) =>
            {
                Log("Basic");
                foreach (string str in DescriptionDict[ "Basics" ])
                {
                    Log('\t' + str);
                }
                Log("End of Basic");
            }),

            new ConsoleCommand("Help", "", ConsoleCommandType.Basics, (object[] ha) =>
            {
                string var = (string)ha[ 0 ];
                if (DescriptionDict.ContainsKey(var) == false)
                {
                    Log($"There are no Help commands on {var}");
                    return;
                }
                Log(var);
                foreach (string str in DescriptionDict[ var ])
                {
                    Log('\t' + str);
                }
                Log($"End of {var}");
            },
            typeof(string))
            
            );
        }
        #endregion 
    }
    public enum ConsoleCommandType
    {
        Basics,
        Manipulation,
        Player,
        Items,
        Settings,
        C_Settings,
        Server
    }

    public sealed class ConsoleCommand
    {
        public readonly string _commandID;
        public readonly string _commandDescription;

        public readonly ConsoleCommandType _commandType;

        private readonly Action<object[]> _action;
        public readonly bool isActive;
        /// <summary>
        /// For clarity's sake use a "Log/LogError" function to tell the user if it worked/didn't work respectivly
        /// </summary>
        /// <param name="commandID">The name of the command, also what is writen at the start, is to be unique</param>
        /// <param name="commandDescription">A short, yet thorough explanation, shown in the help menu</param>
        /// <param name="usePlayerRef">If the command should have the player as a variable</param>
        /// <param name="useTarget">If the character / item that is currently highlighted should be given as a variable, E.G kill, 
        /// as you would need to know what you are supposed to kill</param>
        /// <param name="action">The object[] contains all variables that you asked for, in the order you asked for them, and also converted correctly, so just convert them to what you need and go ham 
        /// <param name="varTypes">The variables <paramref name="usePlayerRef"/> and <paramref name="useTarget"/> are not to be added here.
        /// However, take them into account as they will populate the first 1-2 slots respectively </param>
        /// EXAMPLE: (int)obj[0] OR obj[0] as int</param>
        public ConsoleCommand(
            string commandID, string commandDescription, ConsoleCommandType commandType,
            bool usePlayerRef, bool useTarget, 
            Action<object[]> action, params Type[] varTypes)
        {
            _commandDescription = commandDescription;
            _commandType = commandType;
            _commandID = commandID;
            _usePlayerRef = usePlayerRef;
            _useTarget = useTarget;
            _varTypes = varTypes;
            _action = action;
        }
        /// <summary>
        /// For clarity's sake use a "Log/LogError" function to tell the user if it worked/didn't work respectivly
        /// </summary>
        /// <param name="commandID">The name of the command, also what is writen at the start, is to be unique</param>
        /// <param name="commandDescription">A short, yet thorough explanation, shown in the help menu</param>
        /// <param name="action">The object[] contains all variables that you asked for, in the order you asked for them, and also converted correctly, so just convert them to what you need and go ham 
        /// <param name="varTypes">Add the amount and type of variables that you want, using Types </param>
        /// EXAMPLE: (int)obj[0] OR obj[0] as int</param>
        public ConsoleCommand(
            string commandID, string commandDescription, ConsoleCommandType commandType,
            Action<object[]> action, params Type[] varTypes)
        {
            _commandDescription = commandDescription;
            _commandType = commandType;
            _commandID = commandID;
            _usePlayerRef = false;
            _useTarget = false;
            _varTypes = varTypes;
            _action = action;
        }
        /// <summary>
        /// If this is checked, then the first variable is a reference to the player
        /// </summary>
        public readonly bool _usePlayerRef;
        /// <summary>
        /// If this is checked, then the first variable is a reference to the selected target, 
        /// If the "<seealso cref="_usePlayerRef"/>" is also checked, it becomes the second variable
        /// </summary>
        public readonly bool _useTarget;
        public readonly Type[] _varTypes;
        public void Execute(object[] v1) => _action.Invoke(v1);
    }
}
public interface IHealth
{
    public void Kill();
}