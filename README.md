# maple 🍁

Terminal text editor written in C#

## Getting started

`cd [maple directory]`

`dotnet run -- test.txt`

## Commands

Execute commands by toggling to command input with the `escape` key.

**`help`:** display a list of maple commands

**`close`:** close maple without saving

**`save`:** save the currently open file

Some commands may display an output upon completion. Clear command output with the `escape` key.
It is necessary to clear command output before toggling to the command input again.

## Themes

Maple supports simple syntax highlighting for `.cs` files by default.

To add syntax highlighting for another language:
 - Create a new `.txt` file in the `themes` directory (e.g.: `cs.txt`, `java.txt`)
 - Add each highlighted keyword on a new line
 - Maple will use the highlighting file the next time a file of that type is loaded

Maple also supports custom colors for highlighting, accents, etc:
 - Open `maple.txt` within the `themes` directory
 - Assign any valid Windows console color (all lowercase, no spaces) to each text type (for example: `accent:darkcyan`)
 - Maple will use the set color scheme the next time it is launched
