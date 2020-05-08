# DD2AML-Converter

Welcome to DD2AML! This project was developed for GSD as a student project by (in alphabetical order)

1. [Nico Dietz](https://github.com/dillasyx)
2. [Steffen Gerdes](https://github.com/SteffenGerdes)
3. [Constantin Ruhdorfer](https://github.com/ConstantinRuhdorfer)
4. [Jonas Komarek](https://github.com/JonasKomarek)
5. [Vu Quang Phuc](https://github.com/VuQuangPhuc)
6. [Michael Weidmann](https://github.com/michaelweidmann)

and was further developed for IODD and CSP+ files by (in alphabetical order)

1. [Nora Baitinger](https://github.com/naboga)
2. [Lara Mack](https://github.com/Sophelec)
3. [Bastiane Storz](https://github.com/Maruny)
4. [Antonia Wermerskirch](https://github.com/WAntonia)

at [Baden-Wuerttemberg Cooperative State University (DHBW) Stuttgart](https://www.dhbw-stuttgart.de/home/) under supervision of [Markus Rentschler](http://wwwlehre.dhbw-stuttgart.de/~rentschler/) and Christian Ewertz.

This project is distributed via:

1. [GitHub](https://github.com/WAntonia/TINF18C_Team_3_DD2AML-Converter)


## About this project

This project was developed in .Net Framework 4.7.
This project converts Profinet (PN-)GSD, CSP+ and IODD files to AutomationML.
There are several ways to use this project:

1. GUI
2. CLI
3. Implement the library for your own purposes

You can get an installer or a portable version on the appropiate GitHub [release page](https://github.com/WAntonia/TINF18C_Team_3_DD2AML-Converter/...).

## Contributing to this project

Contributions are always welcome!
If you want to contribute feel free to fork this repo and later perform a pull request.

## Subprojects

The seperate subprojects are explained here and can be found under /src/.

### DD2Aml.Lib

The library contains all relevant logic for:

1. checking whether the GSD, IODD and CSP+ files are valid
2. performing the conversion and either
    * returns a string containing the content of the AML file
    * or converting the GSD, CSP+ or IODD file into an .aml file and including all its dependencies, storing it in an .amlx package. This process uses the [AML.Engine].

For more information, including the relevant conversion rules, see the [library] readme file (located at: src/DdAml.Lib/README.md).


### DD2Aml.Gui

Contains the code that makes up the CLI.
The Gui gives the user access to the functionality of the library without the user having to implement the library himself.
It contains additional functions such as path checking, various selection options, and visual information for the user about events.

<img width="900" alt="GUI" src="https://user-images.githubusercontent.com/50714940/81333452-f0f75a00-90a4-11ea-8249-86deee0ac1c8.jpg">

### Dd2Aml.Cli

Contains the code that makes up the CLI.
This includes parsing and handeling the arguments.
The CLI gives the user access to the functonality of the library without requiring the user to implement the library for themselfs.

It comes with various flags to handle the conversion:

<img width="900" alt="CLI" src="https://user-images.githubusercontent.com/50714940/81333644-2e5be780-90a5-11ea-8f0a-46017e26153f.png">

### Dd2Aml.Test

Contains the code for all unit tests.
The unit tests were build with Microsoft Unit Test Framework.


