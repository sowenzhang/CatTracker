# Overview

This is a naive approach to demonstrate how to use a multiclass classification to identify the breed of a cat. 

It is NOT a realiable solution and it has no intention to be one. 

The motivation of this work is at here: 

[How to identify the breed of a cat](http://hongincanada.com/blog/2020/01/42-version-1-how¡­e-breed-of-a-cat/)

# Before you run

Please update the 2 TSV files: 

- cats.tsv: under folder `Resources\cats` 
- test.tsv: under folder `Resources\testImages` 

You have to specify the absolute path to match your local machine where you store those images. 

# Execution 

The trained model is over 120MB so I cannot push to git. So you have to train the model locally first. 

There are 2 ways to run the console app. 

## 1. Use VS.NET community (easier)

Debug the **CatTrackerML.ConsoleApp** with an argument `train`, which will use some default arguments. 

Next debug the **CatTrackerML.ConsoleApp** with an argument `predict`, which will again use some default arguments. 

## 2. Build it with command line 

Navigate to the root folder, and run this: 

```
dotnet publish -c Release -r win10-x64
```

This will create the exe file under a folder like: 

```
{YOUR_ROOT}\CatTrackerML.ConsoleApp\bin\Release\netcoreapp2.1\win10-x64
```

You can copy all files from the subfolder `win10-x64` to `netcoreapp2.1`, so you can run it with default arguments; otherwise try 

```
CatTrackerML.ConsoleApp.exe -h

CatTrackerML.ConsoleApp.exe train -h

CatTrackerML.ConsoleApp.exe predict -h
```

Each will explain how to use. It's mostly vanila [ML.NET](https://github.com/dotnet/machinelearning) stuff. 

# Image Copyright

All the images are randomly downloaded from internet. If any violate any copyright, please let me know and I am happy to remove them. 

