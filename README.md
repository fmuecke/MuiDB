# Multilingual User Interface strings DataBase (MuiDB) Tool

*Muidb* is a xml-based translation string database. It supports multiple languages per item and separate
translation states. Comments and untranslatable items are supported as well. The translation items are
stored in a single file to simplify access.

The main purpose of *muidb* is to ***simplify the translation workflow***. 

Conversion to and from *muidb* is supported via `muidb-tool.exe` for the following formats
  - XLIFF ([Localisation Interchange File Format](https://www.oasis-open.org/committees/tc_home.php?wg_abbrev=xliff)): 
  [v1.2](http://docs.oasis-open.org/xliff/xliff-core/xliff-core.html) and [v2.0](http://docs.oasis-open.org/xliff/xliff-core/v2.0/os/xliff-core-v2.0-os.html)
  - c# resource files (.resx)  

## Overview

- [Why MuiDB simply works™](#muidb-simply-works)
- [The typical c# translation workflow](#typical-workflow) and [why it sucks](#why-it-sucks)
- [The MuiDB way](#muidb-workflow) 
- [Features overview](#features-overview)
- [Examples](#examples)
- [Configure C# project for muidb](#configure-project)
- [License](#license)


## Available on [nuget.org](https://www.nuget.org/packages/fmdev.MuiDB.Tool/)
[![NuGet version](https://badge.fury.io/nu/fmdev.MuiDB.Tool.svg)](https://badge.fury.io/nu/fmdev.MuiDB.Tool)

To install `muidb-tool`, run the following command in the Package Manager Console

    Install-Package fmdev.MuiDB.Tool

## Why MuiDB simply works™ <a name="muidb-simply-works"></a>
- All translations are stored in one single file - that's all you need to keep track of.
- All other files are/can be generated.
- It supports standard formats (XLIFF, ResX) to export and import strings to/from.
- It keeps strings in sorted order to avoid merge conflicts.
- It's just simple - No complex workflow needed.
- It's all open source. If something should be broken, you can fix it.
- The source translations are always up-to-date.
- No line ending inconsistency magic - it just does what you expect. 

## The typical translation workflow for a shared C#-Project <a name="typical-workflow"></a>

Let's assume we want to ship our application with two languages *en* and *de* and 
want to track the translation process for both.

We then assume that the original development team also provides *both* initial translations:
  - In smaller companies you just don't have the resources
  - Agile software development teams don't want to be dependent upon external resources/processes
  - At least for non English native speakers it should not be a problem to support their mother
   tongue and English as a target language 

Therefore we usually end up having at least three(!) languages in our project:
  - developer/source/fallback language: *"en"*
  - target languages: *en*, *de*

So what does an actual workflow look like?

1. Add the new string resource in Visual Studio
2. Import new string into translation database
3. Update target translations in translation database
4. Write target resource files
5. Submit new translations to version control  
    - Work around tool limitations<sup>[1](#tool-limitations)</sup> 
    - Resolve merge conflicts  
    - Recompile project
6. Review translations
7. (Sync final translation back to source)<sup>[2](#sync-back-footnote)</sup>

<small>
<a name="tool-limitations">(1)</a> Some tools mess up line endings, add unnecessary whitespace, 
use non-standard xml formatting, add proprietary data that prevents merging entirely (like checksums), 
modify a lot of other files which need to be reset
<a name="sync-back-footnote">(2)</a> Some teams wish to have their resource stings in snyc with their 
translation. However most tools do not support syncing back to the source.
</small>


*Which files are involved?*

>`Strings.resx` - the source string database  
>`Strings.en.resx` - the English target translation database   
>`Strings.de.resx` - the German target translation database  
>`Strings.en.xlf` - the English translation database  
>`Strings.de.xlf` - the German translation database

*Which files are involved in a merge conflicts?*

> Apparently **all five of them**. Because almost every time a new string is added it is appended to the *end*
 of the database. And this is also true for all the co-workers adding strings for this project every day. 


### Why does this suck? <a name="why-it-sucks"></a>
It sucks because you have to 
- perform an awful amount of tasks (hopefully in the correct order)
- use at least two different tools (not counting git) 
- resolve merge conflicts
- resolve merge conflicts for 5+ files
- circumvent limitations (bugs) of proprietary tools
- perform time consuming tasks
- touch five(!) files just to add a new string
- do really a lot of work for a simple every-day-task

>*And all of this just to add one simple string resource? - That's just insane!*   


## The MuiDB way<a name="muidb-workflow"></a>
We assume we already [configured muidb-support](#configure-project) for our C# project.
The steps to add a new string are: 

1. Add the new string resource in `muidb.xml` - including initial translations 
2. Auto-generate all resource files 
3. Submit new translations to version control  
6. Review translations

We **do not have to**

- use multiple tools
- work around tool limitations
- resolve merge conflicts
- wait for recompilation

And of course all source strings are always in sync with their translation.

*Which files are involved?*

> `muidb.xml` - the *muidb* string database (includes all translations and translation states)

*Which files are involved in a merge conflicts?*

> **None**. Because muidb stores all strings in alphabetical order, new strings are added almost 
randomly in the string database. And this is as well true for all the co-workers adding strings for this
 project every day ;-)


## Features Overview
- Import and export from/to standard translation/resource files
   - XLIFF v1.1/1.2
   - C# resource (.resx)
- All translations together in one simple xml file
- Can generate all dependent .rex resource files on the fly
- Can be edited with every standard text editor - no fancy tools needed
- Supports translation states
- Merge friendly format
  <!-- with automatic sorting of translation strings (avoids merge conflicts when used within a shared project)-->
- Supports an own comment field per language and translation entry


## Examples

### Sample muidb.xml
See [sample.muidb.xml](sample.muidb.xml).

## Configure C# project for MuiDB <a name="configure-project"></a>
### Muidb configuration/`muidb.xml`

#### `<files>` section
```xml
 <files>
    <resx lang="*">Strings.resx</resx>
    <resx lang="*">Strings.en.resx</resx>
    <resx lang="de">Strings.de.resx</resx>
  </files>
```

- The resx files that will be exported
- what language will be used for each export

#### `<translations>` section
```xml
<translations>
  <item id="ResourceID">
    <text lang="*" state="final">international translation/fallback</text>
    <text lang="de" state="final">translation in DE</text>
  </item>
  ...
</translation>
```

***...TODO...***

### Project file configuration
***...TODO...***

## License
For further license information please consult the [LICENSE](LICENSE) file.  

### Additional credits
Muidb uses the following third party open source libraries:
  - [XliffParser](https://github.com/fmuecke/XliffParser) - XLIFF markup wrapper (BSD license)
  - [ArgsParser](https://github.com/fmuecke/ArgsParser) - a command line parser (BSD license)

And of course those programs deserve credit as well:
  - Visual Studio/[Visual Studio Code](https://code.visualstudio.com)
  - [git for Windows](https://git-scm.com)
  - [nuget](https://nuget.org)
  - [GitHub](https://github.com/)
  
