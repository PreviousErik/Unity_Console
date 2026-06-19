// Copyright Erik Torenstam 2024-2025
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        protected static HZR_Console instance;

        private bool logDeveloperCommands;

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

        private static Dictionary<string, ConsoleCommand> ConComDict;
        private static Dictionary<string, List<string>> DescriptionDict;

        ConsoleControlls inputActions;
		private void OnDestroy()
		{
			inputActions.Disable();
			inputActions.Dispose();
		}
		/// <summary>
		/// PastEntries are what has been entered into the console before, you can also add some default entries here, to make some console commands easely available if they are used a lot.
		/// This has a max of 10 entries
		/// </summary>
		protected static List<string> pastEntries;
        /// <summary>
        /// The consoleLog contains all the logs that has been called, either from a script or information from the 
        /// </summary>
        protected static List<ConsoleEntry> consoleLog;

        private int shownEntry = 0;
        private string field;
        private bool justMarried;
        private bool updateSearchResult;
        private List<string> seartchResults = new List<string>();
        private Trie lookupTable = new Trie();

        private GameObject clickedObject;

        #region Subscription

        private event Action<bool> ConsoleToggleUpdate;

        /// <summary>
        /// Subscribe to get the updates on if the console is being open or not
        /// </summary>
        /// <param name="_func"></param>
        public static void SubscribeToToggle(Action<bool> _func)
        {
            instance.ConsoleToggleUpdate += _func;
            _func.Invoke(instance.consoleActive); // send the update to keep them in the loop if its already open
        }

        public static void UnsubscribeToToggle(Action<bool> _func) => instance.ConsoleToggleUpdate -= _func; 

        #endregion

        #region Console Basics

        /// <summary>
        /// This sets up a lot of the console, if you override this, make sure to call "base.Init()"
        /// </summary>
        protected virtual void Init()
        {
            lookupTable = new Trie();
            inputActions = new ConsoleControlls();
            inputActions.Console.Enable();
            inputActions.Console.OpenConsole.started            += ToggleConsole;
            inputActions.Console.ChoosePreviousInput.started    += DirectionThings;
            inputActions.Console.Enter.started                  += ProcessConsoleEntry;
            inputActions.Console.Mouse.started                  += HandleGettingTargetReference;

            ConComDict = new Dictionary<string, ConsoleCommand>();
            DescriptionDict = new Dictionary<string, List<string>>();

            consoleLog = new List<ConsoleEntry>();
            pastEntries = new List<string>();

			logDeveloperCommands = Application.isEditor || Debug.isDebugBuild;
            //RegisterAttributedMethods();
		}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CreateConsoleInstance()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif
            List<Type> children = typeof(HZR_Console).GetChildClasses();

            if (children.Count == 0)
            {
                Debug.LogWarning("No custom class that inherits from HZR_Console exists");
                return;
            }

            if (children.Count > 1)
            {
                Debug.LogWarning("There are more than one class that inherits from the HZR_Console, only one should exist");
                return;
            }
            if (instance)
            {
                Destroy(instance);
            }
            GameObject obj = new GameObject(children[0].Name);
            obj.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(obj);
            instance = (HZR_Console)obj.AddComponent(children[0]);
            instance.Init();
        }

        private void OnGUI()
        {
            int yPos = (Screen.height / 3) - 40;
            if (consoleActive == false)
            {
                foreach (ConsoleEntry Log in consoleLog)
                {
                    float diff = Log.timeStamp + 10 - Time.time;
                    if (diff < 0) return;
                    DrawEntry(Log, diff);
                }
                return;
            }

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
                updateSearchResult = true; 
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
			GUI.contentColor = Color.white;

			GUI.backgroundColor = Color.clear;
            
            foreach (ConsoleEntry log in consoleLog)
            {
                DrawEntry(log);
            }
            
            if (justMarried)
            {
                // move the carret to the end of the textfield
                TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                editor.cursorIndex = field.Length;
                editor.selectIndex = field.Length;
            }
           
            if (field.Length > 0 && field.StartsWith('/'))
            {
                int yPosition = (Screen.height / 3) - 20;
                if (updateSearchResult == true)
                {
                    updateSearchResult = false;
                    seartchResults = lookupTable.StartsWith(field[1..]);
                }
                if (seartchResults.Count > 0)
                {
                    int shownOptions = seartchResults.Count; //How many options should be shown when you search
                    GUI.backgroundColor = Color.black;
                    int boxSize = shownOptions * 15;
                    GUI.Box(new Rect(0, yPosition - boxSize - 5, Screen.width, boxSize + 10), "");
                    for (int i = 0; i < shownOptions; i++)
                    {
                        GUI.contentColor = -i == shownEntry + 1 ? Color.yellow : Color.white;
                        yPosition -= 15;
                        GUI.Label(new Rect(0, yPosition - 5, Screen.width, 20), seartchResults[i]);
                    }
                }
            }
            
            void DrawEntry(ConsoleEntry log, float diff = 1)
            {
                if (logDeveloperCommands == false && log.onlyDev)
                    return;
				int xPos = 0;
				foreach (var entry in log.Entries)
				{
					if (string.IsNullOrEmpty(entry.text))
					{
						GUI.Label(new Rect(xPos, yPos, Screen.width, 20), entry.img);
                        xPos += 16;
						continue;
					}
					GUIStyle style = new GUIStyle();
                    Color temp = entry.textColor;
                    temp.a = diff;
					style.normal.textColor = temp;
					// TODO: apply more styles here in the future :D
					GUIContent content = new GUIContent(entry.text);
                    
					GUI.Label(new Rect(xPos, yPos, Screen.width, 20), content, style);
					xPos += (int)style.CalcSize(content).x;
				}
				yPos -= 15;
			}
        }

        private void HandleGettingTargetReference(InputAction.CallbackContext _context)
        {
            if (consoleActive == false) 
                return;
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject() == true)
                return; // pointer over UI

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                clickedObject = hit.collider.gameObject;
                EditorLog(new ConsoleEntry.EntrySegment(Color.white, "Clicked on"), new ConsoleEntry.EntrySegment(Color.green, clickedObject.name) );
            }
        }

        private void ChooseThing(InputAction.CallbackContext _context)
        {
            field = pastEntries[ shownEntry -1 ];
            // Removes the added context before sending it to the console
            if (field.Contains('|'))
                field = field[..field.IndexOf('|')];
			shownEntry = 0;
            justMarried = true;
        }

        private void ChooseOtherThing(InputAction.CallbackContext _context)
        {
            field = seartchResults[ (-shownEntry) - 1 ];
            // Removes the added context before sending it to the console
			if (field.Contains('['))
				field = field[..field.IndexOf('[')];
			field = '/' + field;
			shownEntry = 0;
            justMarried = true;
        }
        
        private void ToggleConsole(InputAction.CallbackContext _context) => ToggleConsole();

        private void ToggleConsole()
        {
            if (ConsoleActive == false)
                TurnOnConsole();
            else
                TurnOffConsole();
		}

        protected virtual void TurnOnConsole()
        {
            inputActions.Console.ChoosePreviousInput.Enable();
            inputActions.Console.Enter.Enable();
            shownEntry = 0;
            field = string.Empty;
            ConsoleActive = true;

		}

		protected virtual void TurnOffConsole()
        {
            inputActions.Console.ChoosePreviousInput.Disable();
            inputActions.Console.Enter.Disable();
            ConsoleActive = false;
		}

        private void DirectionThings(InputAction.CallbackContext _context)
        {
            shownEntry = Mathf.Clamp(shownEntry - (int)_context.ReadValue<float>(), -seartchResults.Count, pastEntries.Count);
        }

		#endregion

		#region Logging functionality

		#region Log

		public static void LogError(object _message) => Log(_message.ToString(), Color.red);

        public static void LogWarning(object _message) => Log(_message.ToString(), Color.yellow);

        public static void Log(object _message) => Log(_message.ToString(), Color.white);

		private static void Log(object _message, Color _messageColor)
        {
            consoleLog.Insert(0, new ConsoleEntry(false, new ConsoleEntry.EntrySegment(_messageColor, _message.ToString())));
            if (consoleLog.Count > 20)
                consoleLog.RemoveAt(consoleLog.Count - 1);
        }

        public static void Log(params ConsoleEntry.EntrySegment[] segments)
        {
            consoleLog.Insert(0, new ConsoleEntry(false, segments));
            if (consoleLog.Count > 20)
                consoleLog.RemoveAt(consoleLog.Count - 1);

        }

		#endregion

		#region EditorLog

        public static void EditorLogError(string _message) => EditorLog(_message, Color.red);
        public static void EditorLogError(object _message) => EditorLog(_message.ToString(), Color.red);

        public static void EditorLogWarning(string _message) => EditorLog(_message, Color.yellow);
        public static void EditorLogWarning(object _message) => EditorLog(_message.ToString(), Color.yellow);

        public static void EditorLog(string _message) => EditorLog(_message, Color.white);
        public static void EditorLog(object _message) => EditorLog(_message.ToString(), Color.white);

        public static void EditorLog(string _message, Color _messageColor)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                return;
#endif

            consoleLog.Insert(0, new ConsoleEntry(false, new ConsoleEntry.EntrySegment(Color.green, "-_[EDITOR]_- "), new ConsoleEntry.EntrySegment(_messageColor, _message)));
            if (consoleLog.Count > 20)
                consoleLog.RemoveAt(consoleLog.Count - 1);
        }

        public static void EditorLog(params ConsoleEntry.EntrySegment[] segments)
        {
            List<ConsoleEntry.EntrySegment> withEditor = new List<ConsoleEntry.EntrySegment>(segments);
            withEditor.Insert(0, new ConsoleEntry.EntrySegment(Color.green, "-_[EDITOR]_- "));
            consoleLog.Insert(0, new ConsoleEntry(true, withEditor.ToArray()));
            if (consoleLog.Count > 20)
                consoleLog.RemoveAt(consoleLog.Count - 1);
        }
		#endregion

		#region

		/*public static void RegisterAttributedMethods()
		{
            TypeCache.MethodCollection methods = TypeCache.GetMethodsWithAttribute<Log>();

			foreach (var method in methods)
			{
				Log logAtt = method.GetCustomAttribute<Log>();
				if (logAtt != null)
				{
                    AddCommand(ConsoleCommand.CreateCommand(logAtt.name, logAtt.description, ConsoleCommandType.Basics, Delegate.CreateDelegate(method.GetType(), method)));
				}
			}
            methods = TypeCache.GetMethodsWithAttribute<EditorLog>();

			foreach (var method in methods)
			{
				EditorLog logAtt = method.GetCustomAttribute<EditorLog>();
				if (logAtt != null)
				{
                    AddCommand(ConsoleCommand.CreateCommand(logAtt.name, logAtt.description, ConsoleCommandType.Basics, Delegate.CreateDelegate(method.GetType(), method)));
				}
			}
		}*/

		#endregion

		#endregion

		#region Command processing

		private void ProcessConsoleEntry(InputAction.CallbackContext context)
        {
            if (shownEntry > 0) // If something was chosen from the previous dropdown
            {
                ChooseThing(new InputAction.CallbackContext());
            }
            if (shownEntry < 0)
            {
                ChooseOtherThing(new InputAction.CallbackContext());
                return;
            }


            if (string.IsNullOrEmpty(field))
                return;

            pastEntries.Insert(0, field);
            if (pastEntries.Count > 10)
                pastEntries.RemoveAt(pastEntries.Count - 1);

            if (field.StartsWith('/'))
                ProcessCommand(field[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries));
            else //Just a text thing, for sending messages to others on the server
                UserMessage(SanitizeMessage(field));

            field = "";
        }

        /// <summary>
        /// This function will be called when the user types anything in the console that is not a command
        /// </summary>
        /// <param name="_message"></param>
        protected virtual void UserMessage(string _message) { }
        public static void ProcessDirectCommand(string command) => instance.ProcessCommand(command.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        private void ProcessCommand(string[] parts)
        {

            if (ConComDict.TryGetValue($"{parts[ 0 ]}|{parts.Length - 1}", out ConsoleCommand command) == false)
            {
                LogWarning("The Command you tried to call, does not exist" );
                Log("Type \"Help\" if you need it");
                return;
            }

			// Does litterally nothing since i added the number of arguments in the command
			Type[] varTypes = command._call.GetType().GenericTypeArguments;
			if (varTypes.Length != parts.Length - 1)
            {
                LogWarning($"The Command \"{command._commandID}\" requiers " + (varTypes.Length < parts.Length ? "fewer" : "more") + " variables");
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

        public static string SanitizeMessage(string message)
        {
            message = message.Length > 120 ? message[..120] : message;
            message = Regex.Replace(message,
                @"[^\p{L}\p{N}\p{Sc}\p{Sm}\p{Mn}\p{Pc}\p{Pd}\p{Zs}.,<>{}|_+=!?;:'""-()]",
                string.Empty);

            return message;
        }

        private bool ConvertText(string[] parts, ConsoleCommand command, out List<object> converted)
        {
            converted = new List<object>();
			// TODO: Add a function to click and highlight anything in the scene and use as a reference
			// DONE!
			// TODO: Make it more obvious what you have selected
			Type[] varTypes = command._call.GetType().GenericTypeArguments;
            // make sure to check and add the target
            if (varTypes.Length > 0 && varTypes[0] == typeof(GameObject))
            {
                converted.Add(clickedObject);
            }
			for ( int i = 0; i < parts.Length; i++)
            {
                try
                {
                    converted.Add(Convert.ChangeType(parts[i], varTypes[i]));
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
        /// This function is not to be used, as the Command is not added automatically after creation
        /// </summary>
        public static void AddCommands(params ConsoleCommand[] commands)
        {
            foreach (ConsoleCommand command in commands)
            {
                AddCommand(command);
            }
        }
        /// <summary>
        /// This function is not to be used, as the Command is not added automatically after creation
        /// </summary>
        private static void AddCommand(ConsoleCommand command)
        {
			string ID = command._commandID;
            Type[] varTypes = command._call.GetType().GenericTypeArguments;
			int varCount = varTypes.Length;
            string fullID =  $"{ ID }|{ varCount }";
            // Contains the actuall ID, also using the var count, so you can have multiple types using the same start word
            // as of right now, i dont know a good way of making more variants viable, so for example if you want 2 commands with both the same start word and same count of variables.
            // could make it so that it checks the variables, however this could be problematic as there are a lot of different variables that could be read the same way
            // i could make it so that it only accepts certain types, so you can only use float, and not ulong, short, or int
            // TODO: look into this later IF NECCESSARY!
            // Debug.Log("Added " + fullID);
            if (ConComDict.ContainsKey(fullID)) 
            {
                Debug.LogWarning("The Command you tried to add, allready exists" );
                return;
                
            }

            ConComDict.Add(fullID, command);

            string commandType = command._commandType.ToString();

            string finalDescription = $"Command: { ID }, ";
            string lookupText = ID + ' ';
            int start = 0;
            if (varTypes.Length > 0 && varTypes[0] == typeof(GameObject))
                start++;
			for (int i = start; i < varCount; i++)
            {
                if (i == start)
                {
                    finalDescription += '[';
                    lookupText += '[';
                }
                finalDescription += varTypes[ i ].Name;
                lookupText += varTypes[ i ].Name;
                if (i + 1 == varCount)
                {
                    finalDescription += ']';
                    lookupText += ']';
                    break;
                }
                finalDescription += ", ";
                lookupText += " ";
            }
            instance.lookupTable.Insert(lookupText);
            //Debug.Log(lookupText);
            finalDescription += command._commandDescription;

            // Adds the description and in what category it belonges to
            if (DescriptionDict.ContainsKey(commandType) == false)
                DescriptionDict.Add(commandType, new List<string>() { finalDescription });
            else
                DescriptionDict[ commandType ].Add(finalDescription);
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
        Server_Settings,
        Server
    }

    public sealed class ConsoleEntry
    {
		public readonly EntrySegment[] Entries = new EntrySegment[0];

		public readonly float timeStamp;

        public readonly bool onlyDev;

        public ConsoleEntry(bool onlyDev, params EntrySegment[] entries )
        {
            timeStamp = Time.time;
			Entries = entries;
            this.onlyDev = onlyDev; 
		}

        public ConsoleEntry(bool onlyDev, string text,Color textColor)
        {
            timeStamp = Time.time;
            Entries = new EntrySegment[1] { new EntrySegment(textColor, text )};
            this.onlyDev = onlyDev; 
		}
		
        public readonly struct EntrySegment
        {
            public readonly Color textColor;
            public readonly string text;
            public readonly Texture2D img;

			public static implicit operator EntrySegment((Color textColor, string text) args) 
                => new (args.textColor, args.text);

			public static implicit operator EntrySegment((Color textColor, object thing) args)
			    => new (args.textColor, args.thing);

			public static implicit operator EntrySegment(Texture2D img)
			    => new (img);

			public EntrySegment(Color textColor, string text)
			{
				this.textColor = textColor;
				this.text = text;
				this.img = null;
			}

			public EntrySegment(Texture2D img)
			{
				this.img = img;
				this.textColor = Color.white;
				this.text = string.Empty;
			}

			public EntrySegment(Color textColor, object thing)
			{
				this.textColor = textColor;
				this.text = thing.ToString();
				this.img = null;
			}
		}
    }

    public class TrieNode
    {
        public Dictionary<char, TrieNode> children = new Dictionary<char, TrieNode>();
        public bool IsWord = false;
    }

    public class Trie
    {
        private readonly TrieNode root = new TrieNode();
        //Insert the initial word, will be the base of everything
        public void Insert(string word)
        {
            TrieNode node = root;
            //Takes all the 
            foreach (char c in word)
            {
                if (!node.children.ContainsKey(c))
                    node.children[c] = new TrieNode();
                node = node.children[c];
            }
            node.IsWord = true;
        }
        public Trie(params string[] words)
        {
            foreach (string word in words)
                Insert(word);
        }
        public List<string> StartsWith(string prefix)
        {
            List<string> results = new List<string>();
            TrieNode node = root;

            foreach (char c in prefix)
            {
                if (!node.children.TryGetValue(c, out node))
                    return results;
            }

            DFS(node, prefix, results);
            return results;
        }

        private void DFS(TrieNode node, string prefix, List<string> results)
        {
            if (node.IsWord)
                results.Add(prefix);

            foreach (var kvp in node.children)
            {
                DFS(kvp.Value, prefix + kvp.Key, results);
            }
        }
    }

    public sealed class ConsoleCommand
    {
        public readonly string _commandID;
        public readonly string _commandDescription;

        public readonly ConsoleCommandType _commandType;

        public readonly Delegate _call;
		/// <summary>
		/// OBS!!! THIS DOES NO LONGER WORK!, switch to using <see cref="ConsoleCommand.CreateCommand"/>
		/// </summary>
		[Obsolete()]
        public ConsoleCommand(
            string commandID, string commandDescription, ConsoleCommandType commandType,
            bool usePlayerRef, bool useTarget,
            Action<object[]> action, params Type[] varTypes)
        {
        }
		/// <summary>
		/// OBS!!! THIS DOES NO LONGER WORK!, switch to using <see cref="ConsoleCommand.CreateCommand"/>
		/// </summary>
		[Obsolete()]
        public ConsoleCommand(
            string commandID, string commandDescription, ConsoleCommandType commandType,
            Action<object[]> action, params Type[] varTypes)
        {
        }
        /// <summary>
        /// For clarity's sake use a "Log/LogError" function to tell the user if it worked/didn't work respectivly
        /// </summary>
        /// <param name="commandID">The name of the command, also what is writen at the start, is to be unique</param>
        /// <param name="commandDescription">A short, yet thorough explanation, shown in the help menu</param>
        private ConsoleCommand(
            string commandID, string commandDescription, ConsoleCommandType commandType,
            Delegate call)
        {
            _commandDescription = commandDescription;
            _commandType = commandType;
            _commandID = commandID;
            _call = call;
            HZR_Console.AddCommands(this);
        }
        public static ConsoleCommand CreateCommand(string commandID, string commandDescription, ConsoleCommandType commandType, Action call)
            => new ConsoleCommand(commandID, commandDescription, commandType, call);
        public static ConsoleCommand CreateCommand<T>(string commandID, string commandDescription, ConsoleCommandType commandType, Action<T> call)
            => new ConsoleCommand(commandID, commandDescription, commandType, call);
        public static ConsoleCommand CreateCommand<T1, T2>(string commandID, string commandDescription, ConsoleCommandType commandType, Action<T1, T2> call)
            => new ConsoleCommand(commandID, commandDescription, commandType, call);
        public static ConsoleCommand CreateCommand(string commandID, string commandDescription, ConsoleCommandType commandType, Delegate call)
            => new ConsoleCommand(commandID, commandDescription, commandType, call);
        public void Execute(object[] v1) => _call.DynamicInvoke(v1);
    }
}

/*[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class Log : Attribute
{
    public readonly string name;
    public readonly string description;

    public Log(string name, string description)
    {
		this.name = name;   
        this.description = description;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class EditorLog : PropertyAttribute
{
	public readonly string name;
	public readonly string description;

	public EditorLog(string name, string description)
	{
		this.name = name;
		this.description = description;
	}
}
*/