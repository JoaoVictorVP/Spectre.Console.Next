# Spectre Console Next
This repository contains some powerful UI for Spectre.Console.

Aware of the natural limitations that comes with Spectre interactivity, I made some tricks for being able to show, for example, our file explorer with a search bar, that's is rebuilding the UI inside a live at every change, this works and makes the UI very powerful.

## Usage samples
To prompt user for selecting a file:
```cs
var selected = await FileExplorer.Show("baseDirectory", showRange (defaults to 10));
```
To prevent bugs and other undesirable effects, you should use the async code as is, without running multiple live widgets at the same time (I'm not sure about the effects of this, but some buffer overlaps can occur).

## Migrating
The new version uses a new type called InputSource for handling inputs.
You can use it almost the same way as before, but need to inject the input source.

You can initialize the input source like this:
```cs
var input = new InputSource(AnsiConsole.Console.Input);
input.Run();
```
And then inject it to the widgets (in this example, the file explorer):
```cs
var selected = await FileExplorer.Show(input, "baseDirectory", showRange (defaults to 10));
```

## Showcase
File System Explorer:
![Spectre Console Next File System Explorer](https://user-images.githubusercontent.com/98046863/232319351-18e7eecc-2af9-486f-902a-d2781f222446.gif)

## How to Contribute
Write your widgets, test them manually and make some unit tests for them.
