Working with DNX Projects
=========================

Creating a new project
----------------------

At its simplist form a DNX project is a project.json with a code file:

::

    -MyApp
    --project.json
    --Program.cs

The only mandatory part of a project is that the *project.json* is a valid json file, which means it has at least *{ }* inside it, and that your program.cs is valid C#. You can run a project like the one I have described by navigating to the *MyApp* directory and running ``dnx . run``.

The presence of a *project.json* is what defines a DNX project and it is the *project.json* that defines all the information that the DNX needs to run and package your project. 

When using some editors there are other files (.xproj in the case of Visual Studio), these files are features/requirements of their tool and not a requirement of the DNX. The xproj, for example, is an MSBuild file that is used by Visual Studio and keeps information that is important to Visual Studio, but not impactful for the DNX.

Adding package metadata
-----------------------

Project metadata is information such as the version of your app, author, etc.

To specify this in the *project.json* you create a key for each of the metadata attributes you care about:

.. code-block:: json

    {
      "version": "0.1-alpha",
      "authors": ["John Doe"],
      "description": "A wonderful library that does nice stuff"
    }
    
* Version: The version of the NuGet package and assemblies generated if you pack/publish your application.
* Authors: The authors and owners section of the NuGet packages nuspec.
* Description: The description of the NuGet package.

.. note:: Other metadata can be put into the *project.json*, but the three above are the only ones that will go into the nuspec of the package that is generated.

Webroot
-------

One important concept in ASP.NET 5 is that of the WebRoot. The WebRoot of your application is the location where you will put content to be served to the user, content such as javascript, css, images, etc woul all go inside this folder.

The *webroot* section of the *project.json* specifies which directory is the webroot for this project.

.. code-block:: json

    {
      "webroot": "wwwroot"
    }
    
See :doc:`Understanding ASP.NET 5 Applications <../conceptual-overview/understanding-aspnet5-apps>` for more details on the Webroot.

Including/Excluding Files
-------------------------

By default all code files in a directory containing a *project.json* are included in the project. You can control this with the include/exclude sections of the *project.json*.

Including/Excluding files
^^^^^^^^^^^^^^^^^^^^^^^^^

The most common sections that you will see for including and excluding files are:

.. code-block:: json

    {
      "compile": "*.cs",
      "exclude": [
        "node_modules",
        "bower_components"
      ],
      "publishExclude": [
        "**.xproj",
        "**.user",
        "**.vspscc"
      ]
    }

* The *compile* section means that only .cs files will be compiled.

* The *exclude* section would exclude any files in the node_modules and bower_components directories. Even if they have .cs extensions.

* The *publishExclude* section of the project.json allows you to exclude files from the publish output of your project. In this example all xproj, user, and vspscc files from the output of the publish command. See TODO: :doc:`Publishing DNX Applications <publishing>`

.. note:: Most sections of the project.json that deal with files allow `glob patterns <https://en.wikipedia.org/wiki/Glob_programming>`_, often called wildcards. 

List of include/exclude properties
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

+---------------------+--------------------------------------+-----------------------------------+
| name                | default value                        | remark                            |
+=====================+======================================+===================================+
| compile             |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| compileExclude      |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| content             | ``**/*``                             |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| contentExclude      |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| preprocess          | ``compiler/preprocess/**/*.cs``      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| preprocessExclude   |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| resource            |``compiler/preprocess/resources/**/*``|                                   |
+---------------------+--------------------------------------+-----------------------------------+
| resourceExclude     |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| shared              | ``compiler/shared/**/*.cs``          |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| sharedExclude       |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| bundleExclude       | ``bin/**;obj/**;**/.*/**``           |                                   |
+---------------------+--------------------------------------+-----------------------------------+
| exclude             |                                      |                                   |
+---------------------+--------------------------------------+-----------------------------------+


Advanced Properties
^^^^^^^^^^^^^^^^^^^

In adition to the above table there are some extra properties that you will not use as often.

* The names ending in BuiltIn control the built in values of their associated key. E.g. *compile* always has the value of *compileBuiltIn* appended to it.
* The names ending in Files are ways to specify an individual file, without globbing. These are here so that you can do things like "exclude all files in folder x except this one file that I care about".

+---------------------+-------------------------------------+-----------------------------------+
| name                | default value                       | remark                            |
+=====================+=====================================+===================================+
| compileBuiltIn      | ``**/*.cs``                         | Concatenated to compile.          |
+---------------------+-------------------------------------+-----------------------------------+
| excludeBuiltIn      | ``bin/**;obj/**;*.kproj``           |                                   |
+---------------------+-------------------------------------+-----------------------------------+
| compileFiles        |                                     | Wildcard is not allowed           |
+---------------------+-------------------------------------+-----------------------------------+
| contentFiles        |                                     | Wildcard is not allowed           |
+---------------------+-------------------------------------+-----------------------------------+
| preprocessFiles     |                                     | Wildcard is not allowed           |
+---------------------+-------------------------------------+-----------------------------------+
| resourceFiles       |                                     | Wildecard is not allowed          |
+---------------------+-------------------------------------+-----------------------------------+
| sharedFiles         |                                     | Wildecard is not allowed          |
+---------------------+-------------------------------------+-----------------------------------+

Precedence
^^^^^^^^^^

The sequence of searching are:

1. Gather files from include patterns
2. Exclude files from ignore patterns
3. Exclude files from includes of mutually exclusive types (see below)
4. Adding individually specified files

The following describes the exact lists that are built up with the following notation:

* \+ means included
* \- means excluded
* glob() means the values are used in the globbing algorithm.

::

    CompileList =
      +Glob( +compile +compileBuiltIn -compileExclude -exclude -excludeBuiltIn) 
      -SharedList
      -PreprocessList
      +compileFiles
    
    PreprocessList =
      +Glob( +preprocess -preprocessExclude -exclude -excludeBuiltIn) 
      +preprocessFiles
    
    SharedList =
      +Glob( +shared -sharedExclude -exclude -excludeBuiltIn) 
      +sharedFiles
    
    ResourceList =
      +Glob( +resource -resourceExclude -exclude -excludeBuiltIn) 
      +resourceFiles
    
    ContentList =
      +Glob( +content -contentExclude -exclude -excludeBuiltIn) 
      -CompileList
      -PreprocessList
      -SharedList
      -ResourcesList
      +contentFiles
    
    BundleExcludeList =
      +Glob ( +bundleExclude )

Shared Files
------------

The *shared* section of the project.json is designed to allow you to create a project that shares its source with other projcets, rather than being compiled to a binary.

The scenario where you would most commonly use this is when you have files such as ``sharedAssemblyInfo.cs`` where you want each project that depends on it to compile that file, rather than depend on a binary that exposes a type for you to consume.
 
.. code-block:: json

    {
      "shared": "*.cs"
    }

When you have shared source in your project it will generate a NuGet package with a directory called *shared* containing the shared files. Depending on this package will cause the DNX to compile the code files that are in the shared directory as if they were part of your project.

.. important::

    Because you are adding to the source of a project that depends on your shared code, it is recommended that all the shared code be internal. Having public surface area in the types you are adding to another project is likely to cause problems in the future.

.. note::

    By convention shared project names should end in sources. *Microsoft.AspNet.Common.Sources*, **not** *Microsoft.AspNet.Common*

Commands
--------

When running dnx.exe you can pass the name of a command to execute. For example ``dnx . web`` could be used to run the web command form this example:

.. code-block:: json

    {
      "commands": {
        "web": "Microsoft.AspNet.Hosting server.name=Microsoft.AspNet.Server.WebListener server.urls=http://localhost:5001",
        "kestrel" : "Microsoft.AspNet.Hosting --server Kestrel --server.urls http://localhost:5004",
        "ef": "EntityFramework.Commands"
      }
    }
    
The commands are a set of arguments that will be passed to DNX. The first part of this must be an assembly with a Main entry point that the DNX will try to execute. In the case of the *web* command above the Microsoft.AspNet.Hosting assembly has a main entry point method that is called whenever you run the web command. The rest of the values in the *web* command are used by the hosting assembly to host your web application.

The *ef* command above shows a different assemblies main method being called, in this case the EF assembly doesn't require any extra arguments so it is just the name of the assembly.

The assembly listed in the commands should be pulled in by a package that your application depends on.

Targeting multiple frameworks
-----------------------------

One of the sections you can add to the *project.json* is the ``frameworks`` section. The ``frameworks`` section looks like this

.. code-block:: json

    {
      "frameworks": {
        "dnx451": {},
        "dnxcore50": {}
      }
    }

Each of the values in the ``frameworks`` section of the *project.json* is a framework that your project will compile for. If you pack this project, using ``dnu pack`` then you will get a NuGet package that looks like the following:

.. image:: projects/_static/TargetPlatformNupkg.png

Notice that the nupkg has a folder for each of the frameworks you specified, allowing this NuGet package to work on any of the frameworks you specified. You can also do this

.. code-block:: json

    {
      "frameworks": {
        "dnxcore50":{},
        ".NETPortable,Version=v4.5,Profile=Profile7":{}
      }
    }

With the above ``frameworks`` section you will generate a package with a *portable-net45+win* folder that will be used when running on platforms that match that portable profile.

Dependencies
------------

You manage the dependencies of your application with the ``dependencies`` section of your *project.json*

.. code-block:: json

    {
      "dependencies":{
        "Microsoft.AspNet.Mvc": "6.0.0-beta4"
      }
    }

The above *project.json* section tells the DNX that you depend upon Microsoft.AspNet.Mvc, which means you also depend on everything that the MVC package depends on.

TODO: Add dependency types

Target framework specific dependencies
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You can also add dependencies for a particular framework like this:

.. code-block:: json

    {
      "frameworks": {
        "dnxcore50":{
          "dependencies":{
            "System.Console": "4.0.0.0"
          }
        },
        "dnx451":{}
      }
    }

In the above example the System.Console dependency is only needed for the *dnxcore50* target, not *dnx451*. It is often the case that you will have extra dependencies on Core CLR, because there are packages that you need to depend on in Core CLR that are part of .NET 4.5.x.

.. note:: Whilst it is technically true that you do not need the System.Console package on .NET 4.5.1, it also doesn't matter if you add it as a top level dependency. Each of the System.* packages will work as a top level dependency. So you don't always have to have this seperation, you could add System.Console as a top level dependency and it will not impact your application when on .NET 4.5.1.

Framework Assemblies
^^^^^^^^^^^^^^^^^^^^

You can also specify a list of framework assemblies for frameworks other than *dnxcore50*:

.. code-block:: json

    {
      "frameworks": {
        "dnx451":{
          "frameworkAssemblies":{
            "System.Text": ""
          }
        }
      }
    }

Generally you will use this when you want to depend on an assembly that is in the Global Assembly Cache (GAC) or otherwise part of the framework you are targeting and not a package. The reason that we seperate *frameworkAssemblies* from the rest of the dependencies list is to remove the possibility of accidentally depending on a NuGet package that happens to have the same name as a .NET Framework assembly. 

Restoring packages
------------------

In order to get all the packages that your application depends on you need to restore them from by running ``dnu restore`` in the directory where your *project.json* is. This command will download all the packages required to run, assuming your *project.json* lists everything of course.

The DNX Utility (DNU) wraps the functionaliy of NuGet to do package restore, which means that it uses the NuGet.config file to determine where to download package from. If you want to get packages from somewhere other than NuGet.org you should edit your NuGet.config.

There are instructions on how to get development builds of the latest ASP.NET and DNX packages here: https://github.com/aspnet/Home/wiki/Configuring-the-feed-used-by-dnu-to-restore-packages

Project lock file
-----------------

When doing a package restore DNU builds up a great deal of information about the dependencies of your application, this information is persisted to disk in the project.lock.json.

The DNX reads the lock file when running your application instead of rebuilding all the information that DNU already generated. To understand the reason for that, imagine what the DNX has to do without the lock file:

1. Find each dependency listed in the project.json
2. Open the nuspec of each package and get all of their dependencies
3. Repeat step 2 for each dependency until you have the entire graph.
4. Load all the dependencies.

With the lock file this is reduced to:

1. Read the lock file
2. Load all the dependencies

There is significantly less disk IO involved in the second list.

.. Note:: Because it has such a large impact on the cold start performance of DNX applications the lock file is mandatory to run. If you do not have one the DNX will fail to load your application.

In adition to this DNU also does things like determine if the package should be breadcrumbed for servicing and marks it as such in the lock file. See :doc:`Servicing DNX Applications <servicing>`

Compilation Settings
--------------------

Compilation settings allow you to pass options through to the `.NET Compiler Platform <https://github.com/dotnet/roslyn>`_. The language version can be set here, as well as defines and other options.

.. code-block:: json

    {
      "compilationOptions": {
        "define": ["SOMETHING"],
        "allowUnsafe": true,
        "warningsAsErrors" : true,
        "languageVersion": "experimental"
      }
    }

Configurations
^^^^^^^^^^^^^^

Configurations are named groups of compilation settings. There are 2 defaults built into the runtime, Debug and Release. You can override these (or add more) by adding to the configurations section in the *project.json*.

.. code-block:: json

    {
      "configurations": {
        "Debug": {
          "compilationOptions": {
            "define": ["DEBUG", "TRACE"]
          }
        },
        "Release": {
          "compilationOptions": {
            "define": ["RELEASE", "TRACE"],
            "optimize": true
          }
        }
      }
    }

When building a DNX based application, such as with ``dnu build`` or via pack/publish with ``dnu pack`` or ``dnu publish``, you can pass ``--configuration <configuration>`` to have the DNX use the named configuration.

Scripts
-------

The scripts section of the *project.json* allows you to hook into events that happen as you work on your application:

.. code-block:: json

    {
      "scripts": {
        "prebuild": "executed before building",
        "postbuild": "executed after building",
        "prepack": "executed before packing",
        "postpack": "executed after packing",
        "prepublish": "executed before publish",
        "postpublish": "executed after publish",
        "prerestore": "executed before restoring packages",
        "postrestore": "executed after restoring packages",
        "prepare": "After postrestore but before prepublish"
      }
    }

Most of these are fairly self-explanatory and each matches an explicit command in the DNU. Except for *prepare*. *Prepare* runs both after a restore and before a publish and is intended to be used to make sure everything is ready for either development or publishing. For example, you often need to make sure that you run all of your gulp tasks after you restore packages, to make sure you get things like css copied from new bower packages, and you also want to make sure that gulp is run before you publish so that you are publishing the latest code generated from your tasks.

The values of the scripts are commands that will be run in your environment as if you had opened a terminal and run them. For example, the following is scaffolded when creating a new application in Visual Studio:

.. code-block:: json

    {
      "scripts": {
        "postrestore": [ "npm install", "bower install" ],
        "prepare": [ "gulp copy" ]
      }
    }

Token substitution
^^^^^^^^^^^^^^^^^^

There are also several tokens that will be replaced if they appear inside the scripts value:

==================== =============================
Token                Replaced with                
==================== =============================
%project:Directory%  The project directory
%project:Name%       The project name
%project:Version%    The project version
==================== =============================

If any of the above tokens appear in the value of the script key they will be replaced with the associated value.