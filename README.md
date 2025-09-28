# Unity_Console

Copyright Erik Torenstam 2024-2025

This console is for free use.
You are allowed and in-fact have to create your own extention class of the HZR_Console, to add your own functionality, or overwrite what already exist.

The main focus of this is simple use, but extensive coverage with high malleability.




HOW TO USE!

All you have to do is create your own class that has the HZR_Console that it inherits from, example: 

public class MyConsole : HZR_Console
{

}

from there you are set, as there is nothing more you need to do to get it working, however, this is only the shell, you need to add your own commands, exmaple: 

--------------
ConsoleCommand command = ConsoleCommand.CreateCommand(
"FirstCommand", 
"This is my first command", ConsoleCommandType.Basics, 
() => 
{
	//Here goes your code, will execute on call.
});
 
AddCommands(command);

---------------

The commands supports basic variables, such as int, float, string and similar.
How these are added and utilized is simply adding them in the lambda, and the console will figure the rest out, exmaple:

(string name, int age) =>


If you press the "*" key on your keyboard, and the console will open up, and you can type "/FirstCommand" to call the command "FirstCommand", and each variable that it needs will show up as examples above the inputfield, that also shows what variables it needs.

to call the other variant that has the name and age, you type "/FirstCommand Erik 27" and the console will convert the text into the variables and then call the function using the entered values.

The console will give feedback if a command was not set up properly or was not called using working informaiton or structure. So just make stuff!