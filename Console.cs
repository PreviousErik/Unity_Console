using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Erik.Systems.Console
{
    /* Knows issues
     * If you add a console command, and then destroy/unload the object/script that made it, the command will still be callable, but will cause errors if called
     * Could remove them if they return null i guess ¯\_(ツ)_/¯
    */
    public sealed class Console : MonoBehaviour
    {
        private bool consoleActive = false;
        public static Dictionary<string, ConsoleCommand> ConComDict = new Dictionary<string, ConsoleCommand>();
        public static Dictionary<string, List<string>> DescriptionDict = new Dictionary<string, List<string>>();

        ConsoleInput inputActions;

        private static List<string> pastEntries;
        private static List<ConsoleLogInfo> consoleLog;

        int shownEntry = 0;
        string field;
        bool justMarried;

        #region Console Basics

        private void Awake()
        {
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

        private void Init()
        {
            inputActions = new ConsoleInput();
            inputActions.Console.EnableConsole.Enable();
            inputActions.Console.EnableConsole.performed    += ToggleConsole;
            inputActions.Console.Direction.performed        += DirectionThings;
            inputActions.Console.Enter.performed            += ProcessConsoleEntry;
            inputActions.Console.ChoosePastThing.performed  += ChooseThing;

            ConComDict = new Dictionary<string, ConsoleCommand>();
            DescriptionDict = new Dictionary<string, List<string>>();

            AddServerCommands();
            AddManipulationCommands();
            AddPlayerCommands();
            AddItemsCommands();
            AddSettingsCommands();
            AddBasicCommands();


            consoleLog = new List<ConsoleLogInfo>()
            {
                new ConsoleLogInfo ("Console enabled!",                 Color.white ),
                new ConsoleLogInfo ("Server status: Not started",       Color.white ),
            };
            pastEntries = new List<string>() { "Host", "Host 2", "Help" };
        }

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
            foreach (ConsoleLogInfo Log in consoleLog)
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

        public sealed class ConsoleLogInfo
        {
            public readonly Color textColor;
            public readonly string text;

            public ConsoleLogInfo(string text, Color textColor)
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
        
        private void ToggleConsole(InputAction.CallbackContext context)
        {
            consoleActive = !consoleActive;
            if (consoleActive == true)
            {
                Debug.Log($"The console has been enabled!");
                inputActions.Console.Direction.Enable();
                inputActions.Console.Enter.Enable();
                inputActions.Console.ChoosePastThing.Enable();
                shownEntry = -1;
                field = string.Empty;
            }
            else
            {
                Debug.Log($"The console has been disabled!");
                inputActions.Console.Direction.Disable();
                inputActions.Console.Enter.Disable();
                inputActions.Console.ChoosePastThing.Disable();
            }
        }
        
        private void DirectionThings(InputAction.CallbackContext context)
        {
            shownEntry = Mathf.Clamp(shownEntry - (int)context.ReadValue<float>(), 0, pastEntries.Count);
        }

        #endregion


        #region Log functions

        public static void LogError(string message) => Log(message, Color.red);
        
        public static void LogWarning(string message) => Log(message, Color.yellow);
        
        public static void Log(string message) => Log(message, Color.white);

        public static void Log(string message, Color messageColor)
        {
            consoleLog.Insert(0, new ConsoleLogInfo(message, messageColor));
            if (consoleLog.Count > 20)
                consoleLog.RemoveAt(consoleLog.Count - 1);
        }

        #endregion

        #region Process commands

        private void ProcessConsoleEntry(InputAction.CallbackContext context)
        {
            if (shownEntry > 0)
                ChooseThing(new InputAction.CallbackContext());
            if (string.IsNullOrEmpty(field) == true)
                return;
            pastEntries.Insert(0, field);
            if (pastEntries.Count > 5)
                pastEntries.RemoveAt(pastEntries.Count - 1);

            ProcessCommand(field.Split(' '));

            field = "";

        }
        
        private void ProcessCommand(string[] parts)
        {
            if (ConComDict.TryGetValue($"{parts[0]}|{parts.Length - 1}", out ConsoleCommand command) == false)
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

            if (ConvertText(parts[1..], command, out List<object> converted) == false)
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
            for (int i = 0; i < parts.Length; i++)
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
        private void AddServerCommands()
        {

        }

        private void AddManipulationCommands()
        {

        }

        private void AddPlayerCommands()
        {

        }

        private void AddItemsCommands()
        {

        }

        private void AddSettingsCommands()
        {

        }

        private void AddBasicCommands()
        {

            AddCommands(

            new ConsoleCommand("Test", "Just testing shit", ConsoleCommandType.Basics, (object[] ha) =>
            {
                Debug.Log((int)ha[ 0 ]);
                Debug.Log((float)ha[ 1 ]);
                Debug.Log("Everything went as hoped");
            },
            typeof(int), typeof(float)),

            new ConsoleCommand("AddCommands", "Call with code to activate groups of consolecommands", ConsoleCommandType.Basics, (object[] ha) =>
            {
                Log("This function has not been implemented yet");
            },
            typeof(string)),

            new ConsoleCommand("RobinHood", "Gives the player 1000 gold", ConsoleCommandType.Basics, true, false, (object[] ha) =>
            {
                Log("The player recived 1000 gold!");
            }),

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
            typeof(string)));
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