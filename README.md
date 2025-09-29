# HZR_Console for unity

Copyright Erik Torenstam 2024-2025

This console is for free use.
You are allowed, and in-fact have, to create your own extension class of the HZR_Console, to add your own functionality, or overwrite what already exist.

The main focus of this is simple use, but extensive coverage with high malleability.


## How to use

All you have to do is create your own class that has the HZR_Console that it inherits from, example: 
```
public class MyConsole : HZR_Console
{

}
```
From there you are set, as there is nothing more you need to do to get it working, however, this is only the shell, you need to add your own commands, example: 

```
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

The commands supports basic variables, such as int, float, string, and similar.
How these are added and utilized is simply adding them in the lambda, and the console will figure the rest out, exmaple:

```
(string name, int age) =>
```

If you press the "*" key on your keyboard, and the console will open up, and you can type "/FirstCommand" to call the command "FirstCommand", and each variable that it needs will show up as examples above the inputfield, that also shows what variables it needs.

To call the other variant that has the name and age, you type "/FirstCommand Erik 27" and the console will convert the text into the variables and then call the function using the entered values.

## Context help

There are 2 features that helps you get references to things.

### Player reference

You can get a reference to the player by overriding the GetPlayerReference() in your console extention, and you can use this as a references in your *console-commands*.

> [!NOTE]
> As of right now, this is not implemented fully, and will not work.

### Object reference

You can get a reference to any object by clicking on it while the console is open, you will get a log in the console that will tell you what you last clicked on, and it will be available to reference in your *console-commands*

## Chat

This console can be used as a chat over servers, simply override the "SendMessageToPlayers(string _message)" function, and send the string to the server-manager.

This function has safeguards in place to remove bloat, to make it harder to send an extensive amount of text.

> [!WARNING]
> This is not a garantee that this will work for everything, you have to make sure that this is safe for your game.

> [!NOTE]
> There is no profanity filter.

## To think about

There are also a function-call that will happen when you open up the console, and that is to help you do things, such as show the mouse to make it clickable.
You can subscribe to this call by typing:
```
MyConsole.SubscribeToTurnOn(MyFunction);
```
This will send a bool containing the state of the console.

# Final throughts

The console will give feedback if a command was not set up properly or was not called using working informaiton or structure. So just make stuff!
