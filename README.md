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

## Properties & Switches

User preferences are stored in `properties.xml`, which is read on startup.
Each switch has a corresponding property within `properties.xml`, which can be set to `True`/`False` more permanently.
There are also additional properties which aren't available as switches, (e.g.: `themeFile`).

When running maple, you can include switches to temporarily change editor behavior:

**`--quick-cli`:** `esc` toggles to command input instantly, even if prevous output wasn't cleared
*(if you really want to clear the command output, run `cls`)*

**`--debug-tokens`:** enter tokenizer debug mode *(for development only)*

**`--no-highlight`:** skip the tokenizer and ignore all syntax highlighting rules

## Themes & Syntax Highlighting

Maple supports syntax highlighting for `.cs` files by default.

To add syntax highlighting for another language:
 - Create a new `.xml` file in the `syntax` directory (e.g.: `cs.xml`)
 - Use `<syntax>` tags to define RegEx patterns for different types of tokens (e.g.: `<syntax type="numberLiteral">[0-9]</syntax>`)
 - Use `<keyword>` tags to define different keywords (e.g.: `<keyword>static</keyword>`)
 - Maple will use the highlighting file the next time a file of that type is loaded

Maple also supports custom colors for highlighting, accents, etc:
 - Open `maple.xml` within the `themes` directory
 - Assign any valid Windows console color to each text type (for example: `<color category="accent">darkCyan</color>`)
 - Maple will use the set color scheme the next time it is launched
