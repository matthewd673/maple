# maple 🍁

Terminal text editor for Windows written in C#

**Try it out: `./maple Program.cs`**

***NOTE:** maple works best with [**Windows Terminal**](https://aka.ms/terminal)*

---

## Getting started

`cd [maple project directory]`

`dotnet run -- test.txt`

## Building

Maple is pretty easy to build since it has no dependencies.

`cd [maple project directory]`

`dotnet build -c Release`

## Commands

Execute commands from within maple by toggling to command input with the <kbd>esc</kbd> key.

**`help all`:** display a list of maple commands

**`help [command]`:** get help for a specific command

**`close`:** close maple without saving

**`save [filename]`:** save the currently open file *(optional filename for "save-as")*

**`load [filename]`:** load a new file into the editor *(changes to existing file not saved)*

**`new [filename]`:** create a new file *(changes to existing file not saved)*

**`top`:** move the cursor to the first line of the document

**`bot`:** move the cursor to the last line of the document

**`goto [line number]`:** move the cursor to the given line

**`cls`:** clear previous command output

**`selectin`:** mark the beginning of a selection

**`selectout`:** mark the end of a selection

**`readonly`:** toggle editor readonly mode

**`redraw`:** force a full redraw of the editor, usually fixes any rendering errors

**`syntax`:** render the current file with the syntax rules defined for [extension] files

**`alias [command]`:** view all aliases for a given command

**`url [command]`:** if the cursor is currently hovered on a url, open it in the browser

Some commands may display an output upon completion. Clear command output with the <kbd>esc</kbd> key.
It is necessary to clear command output before toggling to the command input again, unless `--quick-cli` is active.

### Aliases

Maple supports aliases to make entering commands faster or easier to remember. Aliases are contained in `properties/aliases.xml`. Some of the default aliases include **`s`** for `save`, **`go`** for `goto`, and **`ro`** for `readonly`.

## Properties & Switches

User preferences are stored in `properties/properties.xml`, which is read on startup.
Each switch has a corresponding property within `properties.xml`, which can be set to `True`/`False` more permanently.
There are also additional properties which aren't available as switches, (e.g.: `themeFile`).

When running maple, you can include switches to temporarily change editor behavior:

**`--quick-cli`:** `esc` toggles to command input instantly, even if prevous output wasn't cleared
*(if you really want to clear the command output, run `cls`)*

**`--debug-tokens`:** enter tokenizer debug mode *(for development only)*

**`--no-highlight`:** skip the tokenizer and ignore all syntax highlighting rules

**`--cli-no-highlight`:** skip the cli tokenizer and render command input with `cliInputDefault` color

**`--navigate-past-tabs`:** when navigating with the right arrow key, skip past groups of spaces equal to the current tab size

**`--delete-entire-tabs`:** when pressing <kbd>Backspace</kbd> or <kbd>del</kbd>, remove groups of spaces equal to the current tab size

**`--readonly`:** launch the editor in readonly mode

**`--enable-logging`:** log non-fatal errors and internal status in `log.txt`, enabled by default

**`--summarize-log`:** present a summary of important log events when maple closes

## Themes & Syntax Highlighting

Maple supports syntax highlighting for `.cs` files by default, and has the "maple" theme built in.
The syntax and theme systems are fully modular, and custom configurations can be created easily.

Syntax files are loaded based on the filetype of the current document
 - Syntax highlighting files are stored as XML within the `syntax` directory
 - `<syntax>` tags define the RegEx patterns for each supported type of token (e.g.: `<syntax type="numberLiteral">([0-9]+\.?[0-9]*f?)</syntax>`)
   - [regex101.com](https://regex101.com/) makes it significantly easier to develop these syntax rules
 - `<keyword>` tags define the keywords of the language, (e.g.: `<keyword>static</keyword>`)

Theme files are loaded according to the `themeFile` property
 - Theme files are stored as XML within the `themes` directory
 - Each text type can be assigned any valid [Windows console color](https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=net-5.0)
 - Theme categories encompass syntax tokens and maple UI elements, like the footer

## File Nicknames

For quick access to properties, themes, and other maple files there are a few nicknames you can use:

**`{themefile}`:** the theme file currently loaded

**`{propfile}`:** the `properties.xml` file

**`{aliasfile}`:** the `aliases.xml` file

**`{mapledir}`:** the maple directory

**`{themedir}`:** the theme directory

**`{syntaxdir}`:** the syntax directory
