The "seesharp" IRC Bot
==========================

### What Is it?
"seesharp" is an IRC bot that uses the Roslyn CTP to allow you to execute C# code, you can see it on Freenode ( http://webchat.freenode.net/ ) by joining ##csharp-bot

Based on Filip Ekbergs blog post about Roslyn and Hosted Execution ( http://blog.filipekberg.se/2011/12/08/hosted-execution-of-smaller-code-snippets-with-roslyn/  ).

### How does it work?
You need to write the bots name followed by the code / url you want it to process like the following examples.

#### Execute code
	(frW) seesharp, return Enumerable.Range(1,100000).ToArray();
	(seesharp) frW: int[100000] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, ...

#### Execute multiple lines of code
	(frW) seesharp, >> var x = 10;
	(frW) seesharp, return x;
	(seesharp) frW: 10

By using ">>" you can concatenate lines of code

#### Execute code from pastebin.com
	(frW) seesharp, http://pastebin.com/UDKvDLqR
	(seesharp) frW: 10

The code from pastebin.com just included the following:

	var x = 10; return x;
	

### Compiling the code
In order to compile and run the code, read the blog post stated above.

Then you need to setup an SQL Server database and execute the sql queries in "CreateDB.sql"

After that, compile the solution and install the Rossbot Windows Service in the "Rossbot.Windows.Service.Installer" project, after that, you should be able to run RoslynIrcBot.exe from the "RoslynIrcBot" project

### License
All included software is licensed under the MIT License available in full in the LICENSE file.