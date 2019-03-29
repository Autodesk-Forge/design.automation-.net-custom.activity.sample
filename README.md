# Design Automation API for AutoCAD - Create custom Activity and AppPackage


[![.NET](https://img.shields.io/badge/.NET-4.7-green.svg)](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
[![odata](https://img.shields.io/badge/odata-4.0-yellow.svg)](http://www.odata.org/documentation/)
[![Design Automation](https://img.shields.io/badge/Design%20Automation-v3-green.svg)](http://developer.autodesk.com/)
[![visual studio](https://img.shields.io/badge/visual%20studio-2017-yellowgreen.svg)](https://www.visualstudio.com/)
[![License](https://img.shields.io/:license-mit-red.svg)](http://opensource.org/licenses/MIT)

## Description
This is C# sample to demonstrate custom Activities and AppPackages creation. This is the most
common use case that the Design Automation API can run the custom command (defined in the custom package) in the cloud. This sample uses `v3`, for the `v2` sample, please check [design.automation.v2](//github.com/Autodesk-Forge/design.automation-.net-custom.activity.sample/tree/design.automation.v2) branch

## Thumbnail
![thumbnail](/thumbnail.png) 

## Industry Background
* To batch extract layers list and blocks list from the AutoCAD drawing by web services, instead of running plugin in local AutoCAD.

## Setup

### Dependencies 
* Download and install [Visual Studio](https://visualstudio.microsoft.com/downloads/). In the latest test, Visual Studio version is 2017.
* Download and install [AutoCAD SDK](https://www.autodesk.com/developer-network/platform-technologies/autocad). In the latest test with this sample, AutoCAD version is 2018.

### Prerequisites
1. **Forge Account**: Learn how to create a Forge Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/). Make sure to select the service **Design Automation**.
2. Make a note with the credentials (client id and client secret) of the app. 

## Running locally  
1. Build project **CrxApp**. It is better to test with local AutoCAD to verify the process. Steps:
  * Open AutoCAD (in the latest test, the version of AutoCAD is 2018)
  * Open [demo drawing](demofiles/demodrawing.dwg). Run command "netload", select the binary dll of CrxApp. Allow AutoCAD to load it.
  * Run command "test", select [demo json file](demofiles/demojson.json). Specify a output folder. 
  * Finally the blocks name list and layers name list will dumped out.
2. Open project **Client**. Restore the packages of the project by [NuGet](https://www.nuget.org/). The simplest way is
  * VS2012: Projects tab >> Enable NuGet Package Restore. Then right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
  * VS2013/VS2015/2017:  right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
3. Put your Forge credentials into an appsettings.user.json like this
```
{
    "Forge": {
        "ClientId" : "<your client id>",
        "ClientSecret" : "<your client secret>"
    }
}
```
4. Replace the upload URL with your own [here](Client/App.cs#L59).
5. Run project **Client**, you will see a status in the console:
![thumbnail](demofiles/IORunning.png)
6. if everything works well,  the result zip file and the report files will be downloaded to **MyDocuments**. In zip file, blocks list and layers list are available.
7. if there is any error with the process of work item, check the report file what error is indicated. 

## Troubleshooting
* if any problem when building the project [CrxApp](CrxApp), check if the NET Framework of project is compatible with that of AutoCAD References. e.g. with AutoCAD 2018 references, the NET Framework is 4.6.1

## Known Issues
* as of writing, Design Automation of Forge is released with version 2. Odata is used with .NET project. In futher version, OData might not be used. 


## Further Reading 
* [Design Automation API help](https://forge.autodesk.com/en/docs/design-automation/v3/developers_guide/overview/)
 * [ Intro to Design Automation API Video](https://www.youtube.com/watch?v=GWsJM344CJE&t=107s)

## License

These samples are licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

##Written by 

Jonathan Miao & Albert Szilvasy
