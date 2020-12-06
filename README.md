# maple 🍁

Terminal text editor for Windows written in C#

**Try it out: `./maple Program.cs`**

***NOTE:** maple works best with [**Windows Terminal**](https://aka.ms/terminal) - cmd and PowerShell have not been tested*

---

## Getting started

`cd [maple project directory]`

`dotnet run -- test.txt`

## Building

Maple is pretty easy to build since it has no dependencies.

`cd [maple project directory]`

`dotnet build -c Release`

## Commands

Execute commands by toggling to command input with the `escape` key.

**`help`:** display a list of maple commands

**`close`:** close maple without saving

**`save [filename]`:** save the currently open file *(optional filename for "save-as")*

**`load [filename]`:** load a new file into the editor *(changes to existing file not saved)*

**`top`:** move the cursor to the first line of the document

**`bot`:** move the cursor to the last line of the document

**`cls`:** clear previous command output

Some commands may display an output upon completion. Clear command output with the `escape` key.
It is necessary to clear command output before toggling to the command input again, unless `--quick-cli` is active.

## Switches

When running maple, you can include switches to personalize the editor further

**`--quick-cli`:** `esc` toggles to command input instantly, even if prevous output wasn't cleared
*(if you really want to clear the command output, run `cls`)*

**`--debug-tokens`:** enter tokenizer debug mode *(for development only)*

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
