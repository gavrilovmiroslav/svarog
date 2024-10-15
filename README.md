# Svarog

An easy to use, old-school game engine for _olden games_. Think roguelikes, ultimas, etc. Currently only functional as a modular renderer and shader playground.

## Quick Start

To start working with Svarog...
1. clone this repository to your machine
2. open the solution in Visual Studio 2022 or equivalent
3. build all the projects in the solution
4. go to the top folder of the repository and run the `run.bat` batch file!

You're now set. 

Svarog is modular: every piece of functionality is loaded as a separate DLL and can be reloaded: simply rebuild the project you changed while the editor is working and you should see the difference live.

Svarog has a plugin template that makes it very easy to setup a new plugin! Install it by copying the `Svarog Engine Plugin.zip` file from the root of the repo to [where your VS templates are](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-locate-and-organize-project-and-item-templates?view=vs-2022). After that, choose to make a new project in the `svarog` solution, and search for "Svarog". It should pop!

## Plugin 101s

For a plugin to be recognized by the engine, it needs to satisfy two simple conditions:

1. It needs to be a class that has the `[Plugin]` attribute
2. It needs to derive from either `IPlugin` or, more often, `Plugin`. The latter is a class that makes life a bit simpler than if you go with the interface.

Once you have this, simply override the functionalities from the interface/superclass and you're good to go. 
The `[Plugin]` attribute has a `Priority` field which sorts the plugins. The default value for priority is 100. Postprocessing currently starts at 1000.

Plugin lifetimes are straightforward:

1. `Load` - called once either at engine start or when the plugin is loaded
2. `Render` - called every frame during the rendering process
3. `Frame` - called every frame at the end of frame
4. `Unload` - called once either at engine shutdown or when the plugin is unloaded

Plugin reloads are simply `Unload+Load` cycles with no complications.
