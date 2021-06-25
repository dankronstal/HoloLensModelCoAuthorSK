# HoloLensModelCoAuthorSK
This project is a PoC which attempts to showcase the ability to "coauthor" a model using a combination of a HoloLens 2 app developed with [StereoKit](https://www.stereokit.net) and a web UI. 
# Required Environment
Should be pretty quick and easy to pull down the repo and build with Visual Studio 2019. No extras required, as long as you can pull down the NuGet packages for:
- Microsoft.NETCore.UniversalWindowsPlatform (v6.2.9)
- StereoKit (v0.3.1)

A couple of hardcoded spots will need updating in the code, including:
- values for Azure Blob Storage account/container/folder (or whatever other location you'd care to use to store the files. Check the Program.cs and Web/index.html files for the Uri property and update that. 
- autoupdateinterval property to change how often changes sync. Default is 5 seconds.
- model filename. Default is model.csv.
# Running Output
![Video of running code](Assets/demo.gif)
# Notes
Solution needs restructuring, and a few TODOs resolved.
Next step is to add edge and face elements, but for this PoC likely will leave it as single/solid model. More complex implementation will require a total overhaul.