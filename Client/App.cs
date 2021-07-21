using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.Core;
using System.Net;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

namespace Client
{
    class App
    {

        static readonly string PackageName = "MyTestPackage";
        static readonly string ActivityName = "MyTestActivity";
        static readonly string Owner = ""; //e.g. MyTestApp (it must be *globally* unique)
        static readonly string UploadUrl = ""; //e.g. https://dasdev-testing.s3.us-west-2.amazonaws.com/result.zip?X-Amz-Expires=176332&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIZLLKUTZMHO46VTQ/20190120/us-west-2/s3/aws4_request&X-Amz-Date=20190120T042608Z&X-Amz-SignedHeaders=host&X-Amz-Signature=983054fe67ee9575d32fcd293772057b1d246a1d85174924d14468a3b49eb443
        static readonly string Label = "prod";
        static readonly string TargetEngine = "Autodesk.AutoCAD+23";

        DesignAutomationClient api;
        ForgeConfiguration config;
        public App(DesignAutomationClient api, IOptions<ForgeConfiguration> config)
        {
            this.api = api;
            this.config = config.Value;
        }
        public async Task RunAsync()
        {
            if (string.IsNullOrEmpty(Owner))
            {
                Console.WriteLine("Please provide non-empty Owner.");
                return;
            }

            if (string.IsNullOrEmpty(UploadUrl))
            {
                Console.WriteLine("Please provide non-empty UploadUrl.");
                return;
            }

            if (!await SetupOwnerAsync())
            {
                Console.WriteLine("Exiting.");
                return;
            }

            var myApp = await SetupAppBundleAsync();
            var myActivity = await SetupActivityAsync(myApp);

            await SubmitWorkItemAsync(myActivity);
        }

        private async Task SubmitWorkItemAsync(string myActivity)
        {
            Console.WriteLine("Submitting up workitem...");
            var workItemStatus = await api.CreateWorkItemAsync(new WorkItem()
            {
                ActivityId = myActivity,
                Arguments = new Dictionary<string, IArgument>()
                {
                    { "input", new XrefTreeArgument() { Url = "http://download.autodesk.com/us/samplefiles/acad/blocks_and_tables_-_imperial.dwg" } },
                    { "params", new XrefTreeArgument() { Url = $"data:application/json, {JsonConvert.SerializeObject(new CrxApp.Parameters { ExtractBlockNames = true, ExtractLayerNames = true })}" } },
                    //TODO: replace it with your own URL
                    { "result", new XrefTreeArgument() { Verb=Verb.Put, Url = UploadUrl } }
                }
            });

            Console.Write("\tPolling status");
            while (!workItemStatus.Status.IsDone())
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                workItemStatus = await api.GetWorkitemStatusAsync(workItemStatus.Id);
                Console.Write(".");
            }
            Console.WriteLine($"{workItemStatus.Status}.");
            var fname = await DownloadToDocsAsync(workItemStatus.ReportUrl, "Das-report.txt");
            Console.WriteLine($"Downloaded {fname}.");
        }

        private async Task<string> SetupActivityAsync(string myApp)
        {
            Console.WriteLine("Setting up activity...");
            var myActivity = $"{Owner}.{ActivityName}+{Label}";
            var actResponse = await this.api.ActivitiesApi.GetActivityAsync(myActivity, throwOnError: false);
            var activity = new Activity()
            {
                Appbundles = new List<string>()
                    {
                        myApp
                    },
                CommandLine = new List<string>()
                    {
                        $"$(engine.path)\\accoreconsole.exe /i \"$(args[input].path)\" /al \"$(appbundles[{PackageName}].path)\" /s $(settings[script].path)"
                    },
                Engine = TargetEngine,
                Settings = new Dictionary<string, ISetting>()
                    {
                        { "script", new StringSetting() { Value = "_test params.json outputs\n" } }
                    },
                Parameters = new Dictionary<string, Parameter>()
                    {
                        { "input", new Parameter() { Verb= Verb.Get, LocalName = "$(HostDwg)",  Required = true } },
                        { "params", new Parameter() { Verb= Verb.Get, LocalName = "params.json", Required = true} },
                        { "result", new Parameter() { Verb= Verb.Put, Zip= true, LocalName = "outputs", Required= true} }
                    },
                Id = ActivityName
            };
            if (actResponse.HttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Creating activity {myActivity}...");
                await api.CreateActivityAsync(activity, Label);
                return myActivity;
            }
            await actResponse.HttpResponse.EnsureSuccessStatusCodeAsync();
            Console.WriteLine("\tFound existing activity...");
            if (!Equals(activity, actResponse.Content))
            {
                Console.WriteLine($"\tUpdating activity {myActivity}...");
                await api.UpdateActivityAsync(activity, Label);
            }
            return myActivity;

            bool Equals(Activity a, Activity b)
            {
                Console.Write("\tComparing activities...");
                //ignore id and version
                b.Id = a.Id;
                b.Version = a.Version;
                var res = a.ToString() == b.ToString();
                Console.WriteLine(res ? "Same." : "Different");
                return res;
            }
        }

        private async Task<string> SetupAppBundleAsync()
        {
            Console.WriteLine("Setting up appbundle...");
            var myApp = $"{Owner}.{PackageName}+{Label}";
            var appResponse = await this.api.AppBundlesApi.GetAppBundleAsync(myApp, throwOnError: false);
            var app = new AppBundle()
            {
                Engine = TargetEngine,
                Id = PackageName
            };
            var package = CreateZip();
            if (appResponse.HttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"\tCreating appbundle {myApp}...");
                await api.CreateAppBundleAsync(app, Label, package);
                return myApp;
            }
            await appResponse.HttpResponse.EnsureSuccessStatusCodeAsync();
            Console.WriteLine("\tFound existing appbundle...");
            if (! await EqualsAsync(package, appResponse.Content.Package))
            {
                Console.WriteLine($"\tUpdating appbundle {myApp}...");
                await api.UpdateAppBundleAsync(app, Label, package);
            }
            return myApp;

            async Task<bool> EqualsAsync(string a, string b)
            {
                Console.Write("\tComparing bundles...");
                using (var aStream = File.OpenRead(a))
                {
                    var bLocal = await DownloadToDocsAsync(b, "das-appbundle.zip");
                    using (var bStream = File.OpenRead(bLocal))
                    {
                        using (var hasher = SHA256.Create())
                        {
                            var res = hasher.ComputeHash(aStream).SequenceEqual(hasher.ComputeHash(bStream));
                            Console.WriteLine(res ? "Same." : "Different");
                            return res;
                        }
                    }
                }
            }
        }

        private async Task<bool> SetupOwnerAsync()
        {
            Console.WriteLine("Setting up owner...");
            var nickname = await api.GetNicknameAsync("me");
            if (nickname == config.ClientId)
            {
                Console.WriteLine("\tNo nickname for this clientId yet. Attempting to create one...");
                HttpResponseMessage resp;
                resp = await api.ForgeAppsApi.CreateNicknameAsync("me", new NicknameRecord() { Nickname = Owner }, throwOnError: false);
                if (resp.StatusCode == HttpStatusCode.Conflict)
                {
                    Console.WriteLine("\tThere are already resources associated with this clientId or nickname is in use. Please use a different clientId or nickname.");
                    return false;
                }
                await resp.EnsureSuccessStatusCodeAsync();
            }
            return true;
        }
        static string CreateZip()
        {
            Console.WriteLine("\tGenerating autoloader zip...");
            string zip = "package.zip";
            if (File.Exists(zip))
                File.Delete(zip);
            using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
            {
                string bundle = PackageName + ".bundle";
                string name = "PackageContents.xml";
                archive.CreateEntryFromFile(name, Path.Combine(bundle, name));
                name = "CrxApp.dll";
                archive.CreateEntryFromFile(name, Path.Combine(bundle, "Contents", name));
                name = "Newtonsoft.Json.dll";
                archive.CreateEntryFromFile(name, Path.Combine(bundle, "Contents", name));
            }
            return zip;

        }

        static async Task<string> DownloadToDocsAsync(string url, string localFile)
        {
            var fname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), localFile);
            using (var client = new HttpClient())
            {
                var content = (StreamContent)(await client.GetAsync(url)).Content;
                using (var output = System.IO.File.Create(fname))
                {
                    (await content.ReadAsStreamAsync()).CopyTo(output);
                    output.Close();
                }
            }
            return fname;
        }
    }
}
