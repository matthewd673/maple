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

Execute commands from within maple by toggling to command input with the `escape` key.

**`help`:** display a list of maple commands

**`close`:** close maple without saving

**`save [filename]`:** save the currently open file *(optional filename for "save-as")*

**`load [filename]`:** load a new file into the editor *(changes to existing file not saved)*

**`top`:** move the cursor to the first line of the document

**`bot`:** move the cursor to the last line of the document

**`cls`:** clear previous command output

**`selectin` (`i`):** mark the beginning of a selection

**`selectout` (`o`):** mark the end of a selection

**`redraw`:** force a full redraw of the editor *(experimental)*

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

**`--navigate-past-tabs`:** when navigating with the right arrow key, skip past groups of spaces equal to the current tab size

## Themes & Syntax Highlighting

Maple supports syntax highlighting for `.cs` files by default, and has the "maple" theme built in.
The syntax and theme systems are fully modular, and custom configurations can be created easily.

Syntax files are loaded based on the filetype of the current document
 - Syntax highlighting files are stored as XML within the `syntax` directory
 - `<syntax>` tags define the patterns for each supported type of token (e.g.: `<syntax type="numberLiteral">([0-9]+\.?[0-9]*f?)</syntax>`)
 - `<keyword>` tags define the keywords of the language, (e.g.: `<keyword>static</keyword>`)

Theme files are loaded according to the `themeFile` property
 - Theme files are stored as XML within the `themes` directory
 - Each text type can be assigned any valid [Windows console color](https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=net-5.0)
 - Theme categories encompass syntax tokens and maple UI elements, like the footer
