# Multilingual User Interface strings DataBase (MuiDB) Tool

The main purpose of MuiDB is to ***simplify the translation workflow***.


## Available on [nuget.org](https://www.nuget.org/packages/fmdev.MuiDB.Tool/)
[![NuGet version](https://badge.fury.io/nu/fmdev.MuiDB.Tool.svg)](https://badge.fury.io/nu/fmdev.MuiDB.Tool)

To install `muidb-tool`, run the following command in the Package Manager Console

    Install-Package fmdev.MuiDB.Tool

## Overview

- [A typical c# translation workflow](#typical-workflow)
- [The MuiDB way](#muidb-workflow) 
- [Why MuiDB simply works™](#muidb-simply-works)
- [Features overview](#features)
- [Examples](#examples)
- [License](#license)


## What does a typical translation workflow for a shared C#-Project look like?<a name="typical-workflow"></a>

Let's assume we want to ship our application with two languages *en* and *de* and 
want to track the translation process for both.

We then assume that the original development team also provides *both* initial translations:
  - In smaller companies you just don't have the resources
  - Agile software development teams don't want to be dependent upon external resources/processes
  - At least for non English native speakingers it should not be a problem to support their mother
   tongue and English as a target language 

Therefore we usally end up having at least three (!) languages in our project:
  - developer/source/fallback language: *"en"*
  - target languages: *en*, *de*

So what does an actual workflow look like?

1. Define the new string resource in Visual Studio (source language; &rarr; string gets added to `strings.resx`)
2. Import string source into translation database
    - Open your favourite translation application (Multilingual App Toolkit, WinTrans...)
    - Update source file/import string
3. Define initial target translations
    - Open *en* target language file (e.g. `strings.en.xlf`)
    - Define proper English translation for new string
    - Advance translation state
    - Open *de* target language file (e.g. `strings.de.xlf`)
    - Define proper German translation for new string
    - Advance translation state
4. Write target files
    - Write *en* target file (&rarr; `strings.en.resx`)
    - Write *de* target file (&rarr; `strings.de.resx`)
5. Work around tool limitations
    - Fix broken line endings
    - Reformat XML documents
    - Remove proprietary data from translation files that will force even more merge conflicts (WinTrans)
6. Check in initial translations into version control
    - Check out new translations made by your co-workers
    - <span style="color:red">Resolve *merge conflicts for **five** files*</span>:
       - `strings.resx`
       - `string.en.resx`
       - `string.de.resx`
       - `strings.en.xlf`
       - `strings.de.xlf`
    - Recompile
    - Check in translations

And some time later:

1. Review translation
    - Open English translation file (`strings.en.xlf`)
    - Review translation and advance translation state
    - Open German translation file (`strings.de.xlf`)
    - Review translation and advance translation state
2. (Sync back final translation)
    - not possible with standard tools

### Summing it up
- Merge conflicts
- Merge conflicts in 5+ files
- Really a lot of nasty work for a simple every day task
- You have to live with proprietary tools and their limitations (bugs)

>*And all of this just to add one simple string resouce? - That's just insane!*   
> *There had to be a better way...*

## The MuiDB way<a name="muidb-workflow"></a>
We assume we already configured muidb-support for our C# project:
    - resx files that will be exported
    - what languge will be used for each export

1. Define the new string resource in `muidb.xml`
    - Define initial English translation and set translation state
    - Define initial German translation and set translation state
2. <strike>Import string source into translation database</strike>
3. <strike>Define initial target translations</strike>
4. Write target files
    - Call `muidb-tool export -muidb muidb.xml`
    - Automatically generates `strings.resx`, `strings.en.resx`, `strings.de.resx`
5. <strike>Work around tool limitations</strike>
6. Check in initial translations into version control
    - Check out new translations made by your co-workers
    - <span style="color:green">Resolve *no merge conflicts for **one** file:*</span> `muidb.xml`
    - <strike>Recompile</strike>
    - Check in translations

And of course the review steps should be simpler as well. If it should be necessary to have separate files 
for each language - just export them...


## Why MuiDB simply works™ <a name="muidb-simply-works"></a>
- All translations are stored in one single file - that's all you need to keep track of.
- All other files are/can be generated.
- It supports standard formats (XLIFF, ResX) to export and import strings to/from.
- It keeps strings in sorted order to avoid merge conflicts.
- It's just simple - No complex workflow needed.
- It's all open source. If something should be broken, you can fix it.
- The source translations are always up-to-date.

## Features
- Import and export from/to standard translation/resource files
   - XLIFF v1.1/1.2
   - C# resource (.resx)
- All translations together in one simple xml file
- Can generate all dependent .rex resource files on the fly
- Can be edited with every standard text editor - no fancy tools needed
- Supports translation states
- Merge friendly format
   - Automatic sorting of translation strings (avoids merge conflicts when used within a shared project)
- Supports an own comment field per language and translation entry

## Examples

TBD...

## License
For further license information see the [LICENSE](LICENSE) file.
