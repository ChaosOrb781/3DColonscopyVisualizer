# 3DColonscopyVisualizer

The prototype is written in C# and initially prototyped in python, but as video media is quite heavy, then it became too slow to properly perform test iterations.

## Functionality

The prototype is compiled in visual studio and the executables can be found in the prototype.rar file at the root.

A basic usage of the prototype goes as follows when you launch the application. Please note that it uses SQLite to store intermediate data to prevent redundant operations between the stages:

1. "Load video" option opens a file picker, find the sample videos found in the Videos/ folder
2. "Process Frames" select a video that is loaded into the database (has to have been picked through the "Load video" option).
3. "Show Feature Matches" displays frame-for-frame feature matches of a video that was processed using "Process Frames"

To format a video to a workspace to be loaded by ColMap, use the 4th option "Initialize ColMap workspace". This will create subfolders at the selected folder called "Image" and "Mask", where in ColMap's GUI you can then choose the workspace to be the folder you picked and select the images and masks folder accordingly when starting new reconstruction in ColMap. Note: Mask/ is only filled if a .png file of the same name of the video resides in the same folder, this will then get replicated to every frame, such that ColMap can apply it.

The 5th option "Run ColMap reconstruction (windows)" uses the CLI for ColMap, but has proven unstable.
