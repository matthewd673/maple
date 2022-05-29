# maple 🍁

Maple is a terminal-based editor for Windows written in C#. It is intended to be highly modular and customizable without feeling intimidating.

**[Website](http://www.mattdaly.xyz/maple/)** | **[Downloads](https://github.com/matthewd673/maple/releases)** | **[Wiki](https://github.com/matthewd673/maple/wiki)**

***NOTE:** maple works best with [**Windows Terminal**](https://aka.ms/terminal)*

---

## Building

Maple requires [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to build and run. Once that's downloaded, maple is super easy to build:

```
git clone https://github.com/matthewd673/maple.git
cd maple
dotnet run -- Program.cs
```

Maple is now editing itself!

## Documentation

Detailed documentation for maple commands, themes, properties, etc. is available on [the wiki](https://github.com/matthewd673/maple/wiki).

## Language Support

Maple's syntax highlighting is designed to be entirely modular. In other words, maple doesn't provide better support or extra features for any one language over another. However, this repo does include a growing list of languages that are included out of the box: **C#, C, Java, Markdown, and XML**.