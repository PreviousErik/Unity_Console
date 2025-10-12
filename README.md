# HZR_Console for unity

Created by Erik Torenstam ©2024-2025

This console is for free use.
You are allowed, and in-fact have, to create your own extension class of the HZR_Console, to add your own functionality, or overwrite what already exist.

The main focus of this is simple use, but extensive coverage with high malleability.

This will help you create a workflow that is going to make testing incredibly fast and easy.

## Features

### Commands

The main feature of this is the commands, where you can call functions through simply typing in the console, some examples of what can be added:
- Change settings without having to create the UI for it
- Adding cheats :D
- Moving players / objects
- Triggering events
- Giving items and resources
- Spawning characters
- Testing damage- and death-calls
- And so much more

### Search field

When you search for the commands that you have added, it will show you all the potentials along the way, and each variable that is required to complete the call.

<img width="133" height="101" alt="image" src="https://github.com/user-attachments/assets/77931f9f-2abc-4d65-955f-07133609ca01" />

As you type more, this will be narrowed down to the relevant calls that are available.

<img width="156" height="100" alt="image" src="https://github.com/user-attachments/assets/0322499e-68cf-401f-817c-e65d4e266eb2" />

Note that this also shows each variable that is required to call the command, as if you don't add them, the command will not be called.

You do not have to worry about the amount of commands added, as the search can handle over 1000 entries with ease.

> [!IMPORTANT]
> Each entry have to be unique, either with the name or with how many entries that it has. 
> If you have overrides for calls that have the same name, they have to have a unique number of variables.
> Adding different variables or changing the order of the vairables will not work, as it only has to do with the number of them.

These are always available, make sure that the calles have references to everything.

You can keep track of the commands from where they were added, so that you can remove them if the required items are unavailable.

### Choosing stuff

### Descriptions

### Command-types

### Logging

This console has a logging function, where you can give information duing gameplay, there are 2 variants of this, that should be used together for the best results
- Default logging
	This will give the player information about what is going on, such as if someone joined your game, if they have gotten an achievement and anything else you can imagine.

## How to use

All you have to do is create your own class that has the HZR_Console that it inherits from, example: 
```c#
public class MyConsole : HZR_Console
{

}
```
There are a few functions that you can override, but nothing you have to do!

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

> [!WARNING]
> Make sure to add the command, as it is not done automatically

> [!IMPORTANT]
> You do not need to add the console to anything, it will instantiate one when you start the game.
> It will not be destroyed when you load a new scene

## EditorOnly

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

You can get a reference to any object by clicking on it while the console is open, you will get a log in the console that will tell you what you last clicked on, and it will be available to reference in your *console-commands*, example:

```c#
(GameObject clickedObject, string name, int age) =>
```

This will only require 2 variabels when called in the console, but will still add the reference to the clicked object.

### How to use the reference

To have a command that will use the clicked on reference, simply add a GameObject as the first variable in your function, and it will be exchanged for a reference to the gameobject that was clicked on last.

## Chat

This console can be used as a chat over servers, simply override the "SendMessageToPlayers(string _message)" function, and send the string to the server-manager.

This function has safeguards in place to remove bloat, and will make it harder to send an excessive amount of text.

> [!WARNING]
> This is not a guarantee that this will work for everything, you have to make sure that this is safe for your game.

> [!NOTE]
> There is no profanity filter.

## Future things and unfinished areas

## Can not find a good name for this section, so here you go

### What commands exist?
The console will automatically add and keep track of all entries.

### How many commands are to many?
It can easily search through over 1,000 entries lightning-fast.
If you get to the point that you have so many entries/commands that your game starts to slow down when you seach, you have other issues!

### What variables do i need?
The console shows what other variables that each command requires. 

### Toggling the console 
There are also a function-call that will happen when you open up the console, and that is to help you do things, such as show the mouse to make it clickable.
You can subscribe to this call by typing:
```c#
MyConsole.SubscribeToToggle(MyFunction);
```
and to unsubscribe
```c#
MyConsole.UnsubscribeToToggle(MyFunction);
```
This will send a bool containing the state of the console to "MyFunction".

> [!NOTE]
> The console will give feedback if a command was not set up properly or was not called using working informatino or structure. So just make stuff!
