# Design Automation API v2 - C# sample: Create custom Activity and AppPackage
(Formely AutoCAD I/O)

[![.net](https://img.shields.io/badge/.net-4.5-green.svg)](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
[![odata](https://img.shields.io/badge/odata-4.0-yellow.svg)](http://www.odata.org/documentation/)
[![ver](https://img.shields.io/badge/Design%20Automation%20API-2.0-blue.svg)](https://developer.autodesk.com/api/autocadio/v2/)
[![visual studio](https://img.shields.io/badge/Visual%20Studio-2012%7C2013-brightgreen.svg)](https://www.visualstudio.com/)
[![License](https://img.shields.io/:license-mit-red.svg)](http://opensource.org/licenses/MIT)

##Description
This is C# sample to demonstrate custom Activities and AppPackages creation. This is the most
common use case that the Design Automation API can run the custom command (defined in the custom package) on the cloud.

##Dependencies

* Visual Studio 2012, 2013. 2015 should be also fine, but not yet been tested.
* [ObjectARX SDK] (http://usa.autodesk.com/adsk/servlet/index?siteID=123112&id=773204). The SDK version depends on which AutoCAD verison you want to test with the AppPackage locally. In current test, the version is 2016.

##Setup/Usage Instructions
* Unzip ObjectARX SDK. Add AcCoreMgd, AcDbMgd and acdbmgdbrep from SDK/inc to the the project **CrxApp**.  "Copy Local" = False.
* NuGet the Newtonsoft.json package to project **CrxApp**.
* Build project **CrxApp**. It is better to test with local AutoCAD to verify the process. Steps:
  * Open AutoCAD (in this test, the version is 2016)
  * Open [demo drawing](demofiles/demodrawing.dwg). Run command "netload", select the binary dll of CrxApp. Allow AutoCAD to load it.
  * Run command "test", select [demo json file](demofiles/demojson.json). Specify a output folder. 
  * Finally the blocks name list and layers name list will dumped out.
* Restore the packages of project **Client** by [NuGet](https://www.nuget.org/). The simplest way is to right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
* Apply credencials of Design Automation API from https://developer.autodesk.com/. Put your consumer key and secret key at  line 19 and 20 of [program.cs](Client/Program.cs) 
* Run project **Client**, you will see a status in the console:
[![](demofiles/IORunning.png)] 
* if everything works well,  some result files (pdf, zip) and the report files will be downloaded to **MyDocuments**.
* if there is any error with the process, check the report file what error is indicated.

Please refer to [Design Automation API v2 API documentation](https://developer.autodesk.com/en/docs/design-automation/v2/overview/) for more information such as how to setup a project.

## Questions

Please post your question at our [forum](http://forums.autodesk.com/t5/autocad-i-o/bd-p/105).

## License

These samples are licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

##Written by 

Jonathan Miao & Albert Szilvasy
