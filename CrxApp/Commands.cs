using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

[assembly: CommandClass(typeof(CrxApp.Commands))]
[assembly: ExtensionApplication(null)]

namespace CrxApp
{
    public class Parameters
    {
        public bool ExtractBlockNames { get; set; }
        public bool ExtractLayerNames { get; set; }
    }
    public class Commands
    {
        [CommandMethod("MyTestCommands", "test", CommandFlags.Modal)]
        static public void Test()
        {
            //prompt for input json and output folder
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var res1 = ed.GetFileNameForOpen("Specify parameter file");
            if (res1.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;

            var res2 = ed.GetString("Specify output sub-folder name");
            if (res2.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;
            try
            {
                //get parameter from input json
                var parameters = JsonConvert.DeserializeObject<Parameters>(File.ReadAllText(res1.StringResult));
                Directory.CreateDirectory(res2.StringResult);
                //extract layer names and block names from drawing as requested and place the results in the
                //output folder 
                var db = doc.Database;
                if (parameters.ExtractLayerNames)
                {
                    using (var writer = File.CreateText(Path.Combine(res2.StringResult, "layers.txt")))
                    {

                        dynamic layers = db.LayerTableId;
                        foreach (dynamic layer in layers)
                            writer.WriteLine(layer.Name);
                    }
                }
                if (parameters.ExtractBlockNames)
                {
                    using (var writer = File.CreateText(Path.Combine(res2.StringResult, "blocks.txt")))
                    {

                        dynamic blocks = db.BlockTableId;
                        foreach (dynamic block in blocks)
                            writer.WriteLine(block.Name);
                    }
                }
            }
            catch (System.Exception e)
            {
                ed.WriteMessage("Error: {0}", e);
            }
        }
    }
}
