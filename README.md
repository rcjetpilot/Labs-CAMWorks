CAMWorks Plugin for DriveWorks
==============================================

Introduction
-----------------------------------------------
This is sample code which illustrates how to use the DriveWorks API to implement a simple integration into CAMWorks for the automatic generation of CNC data based on a SolidWorks Part.

This code is provided under the MIT license,
for more details see [LICENSE.md](https://github.com/DriveWorks/Labs-CAMWorks/blob/master/LICENSE.md).

This sample code was built using:
- Microsoft Visual Studio 2013
- The DriveWorks 9.3 SDK (technically a standard DriveWorks install (Version 9.3 or later) will also work, but the SDK gives access to help)

This code was tested using:
- DriveWorks Autopilot 11 SP0
- CAMWorks 2014 SP1
- SolidWorks 2014 SP2

This plugin will automatically run CAMWorks operations when a part is created by DriveWorks.

Add a master part name to the list above to ensure any part created from it runs the CAMWorks operations.

Further control is achieved using the following optional custom properties in the part (These custom properties can then be controlled by rules in DriveWorks)

DWCAMWorks - Used to cancel the whole CAMWorks operations for this part.  Set to 'False' to Cancel, any other value (or not present) to proceed.

DWCAMWorksEMF - Used to control the Extract machine Features Operation. Set to 'False' to not run this operation, any other value (or not present) to run.

DWCAMWorksGOP - Used to control the Option for the Generate Option Plan. Set to 2 if custom property is not present. (1:RETAIN, 2:REGENERATE, 3:CANCEL, 4:QUERY PREFERENCES)

DWCAMWorksGTP - Used to control the Generate Tool Path Operation. Set to 'False' to not run this operation, any other value (or not present) to run.

DWCAMWorksPostFilePath - Set to be the path to the Post Process File.  Leave blank to not run the post.  Path can be a full path, or relative to the new part.

DriveWorks will also run Macros before each process and at the end of the last process.  The macros can either be a single macro called DriveWorks-CamWorks.swp located in the Group Content Folder, in a sub folder called Macros and/or be in the same folder as the master model with CamWorks as a file suffix (For instance if the master file is called Block.sldprt then the macro would be called BlockCamWorks.swp).  The macro called DriveWorks-CamWorks.swp is always run first if it exists.

Each macro then needs 6 different macro subs with the names Step1, Step2 ......Step6 which will run at the following times.

- Step1
- Extract Machine Features
- Step2
- Generate Operation Plan
- Step3
- Generate Tool Path
- Step4
- Post Process
- Step5
- Save CAM Data
- Step6

