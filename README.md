# HZR_Console for unity

Created by Erik Torenstam ©2024-2025

This console is for free use.
You are allowed, and in-fact have, to create your own extension class of the HZR_Console, to add your own functionality, or overwrite what already exist.

The main focus of this is simple use, but extensive coverage with high malleability.

This will help you create a workflow that is going to make testing incredibly fast and easy.


## How to use

All you have to do is create your own class that has the HZR_Console that it inherits from, example: 
```c#
public class MyConsole : HZR_Console
{

}
```
From there you are set, as there is nothing more you need to do to get it working, however, this is only the shell, you need to add your own commands, example: 

```c#
ConsoleCommand command = ConsoleCommand.CreateCommand(
	"FirstCommand",
	"This is my first command",
	ConsoleCommandType.Basics, 
	() => 
	{
		//Here goes your code, will execute on call.
	});
 
AddCommands(command);
```

### EditorOnly

If this is supposed to be used as information for the player, such as how it's used in Factorio, then use the Editor-logs.
They work the same as the other logs, but will show in the console that they are for developers only, and will not show up outside of the editor/developer builds!

> [!NOTE]
> I am working on a way to call functions as well, but this can be worked around by simply calling the function in the lambda expression

The commands supports basic variables, such as int, float, string, and similar.
How these are added and utilized is simply adding them in the lambda, and the console will figure the rest out, example:

```c#
(string name, int age) =>
```

If you press the "*" key on your keyboard, and the console will open up, and you can type "/FirstCommand" to call the command "FirstCommand", and each variable that it needs will show up as examples above the inputfield, that also shows what variables it needs.

To call the other variant that has the name and age, you type "/FirstCommand Erik 27" and the console will convert the text into the variables and then call the function using the entered values.

## Object reference

You can get a reference to any object by clicking on it while the console is open, you will get a log in the console that will tell you what you last clicked on, and it will be available to reference in your *console-commands*

### How to use the reference

To have a command that will use the clicked on reference, simply add a GameObject as the first variable in your function, and it will be exchanged for a reference to the gameobject that was clicked on last.

## Chat

This console can be used as a chat over servers, simply override the "SendMessageToPlayers(string _message)" function, and send the string to the server-manager.

This function has safeguards in place to remove bloat, and will make it harder to send an excessive amount of text.

> [!WARNING]
> This is not a guarantee that this will work for everything, you have to make sure that this is safe for your game.

> [!NOTE]
> There is no profanity filter.

## Can not find a good name for this section, so here you go

### What commands exist? / searching
The console will automatiaclly keep track of and add all entries, and you can easily search through over 1,000 entries lightning-fast.

### What variables do i need?
The console shows what other variables that each command requires. 

### Toggling the console 
There are also a function-call that will happen when you open up the console, and that is to help you do things, such as show the mouse to make it clickable.
You can subscribe to this call by typing:
```c#
MyConsole.SubscribeToTurnOn(MyFunction);
```
and to unsubscribe
```c#
MyConsole.UnsubscribeToTurnOn(MyFunction);
```
This will send a bool containing the state of the console to "MyFunction".

> [!NOTE]
> The console will give feedback if a command was not set up properly or was not called using working informatino or structure. So just make stuff!
