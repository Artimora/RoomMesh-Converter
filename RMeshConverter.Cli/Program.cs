using CopperDevs.Logger;
using RMeshConverter.Exporter;
using RMeshConverter.Exporter.Obj;
using RMeshConverter.Exporter.Valve;
using RMeshConverter.RMesh;
using RMeshConverter.XModel;

namespace RMeshConverter.Cli;

public static class Program
{
    public static void Main()
    {
        const string InputFolder = @"C:\Development\scpcb\rmesh-raw";
        const string OutputFolder = @"C:\Development\scpcb\rmesh-out";

        Config.InputFolder = InputFolder;
        Config.Files = Directory.GetFiles(InputFolder, "*.rmesh", SearchOption.AllDirectories);
        Config.ModelFiles = Directory.GetFiles(InputFolder, "*.x", SearchOption.AllDirectories);
        Config.OutputFolder = OutputFolder;
        Config.ModelOutputFolder = Config.OutputFolder;

        Convert();
    }


    public static void Convert()
    {
        foreach (var file in Config.Files)
        {
            try
            {
                var name = file.Split("\\").Last().Replace(".rmesh", "");
                using var reader = new RoomMeshReader(file);
                reader.Read();

                using var writer = GetExporter("WaveFront Obj", name, file, reader);
                writer.Convert();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        foreach (var file in Config.ModelFiles)
        {
            try
            {
                var name = file.Split("\\").Last().Replace(".x", "");
                using var conv = new XAsciiReader(file);
                conv.Convert();
                using var xpr = new XExporter(conv, file, name, $"{Config.ModelOutputFolder}\\Models");
                xpr.Convert();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        GC.Collect();
        Log.Info("Finished Converting.");
    }


    private static Exporter.MeshExporter GetExporter(string exporter, string name, string file, RoomMeshReader reader)
    {
        MeshExporter exp;
        var outputFolder = Config.OutputFolder;

        switch (exporter)
        {
            case "WaveFront Obj":
                Config.ModelOutputFolder = Config.OutputFolder;
                exp = new ObjRoomMeshExporter(name, outputFolder, file, reader);
                break;
            case "FBX (Binary)":
                Config.ModelOutputFolder = Config.OutputFolder;
                exp = new FbxRoomMeshExporter(reader, file, name, outputFolder);
                break;
            case "S&Box Vmdl (Obj)":
                Config.ModelOutputFolder = $"{Config.OutputFolder}/source/";
                exp = new VmdlRoomMeshExporter(reader, file, name, outputFolder, "models", false);
                break;
            case "S&Box Vmdl (FBX Binary)":
                Config.ModelOutputFolder = $"{Config.OutputFolder}/source/";
                // exp = new VmdlExporter(reader, file, relativePath, outputFolder);
                throw new NotImplementedException("S&Box FBX has not yet been implemented");
                break;
            case "S&Box Prefab (Obj)":
                Config.ModelOutputFolder = $"{Config.OutputFolder}/source/";
                exp = new PrefabWriter(reader, file, name, outputFolder, "prefabs/Map");
                break;
            default:
                throw new ArgumentException($"exporter by name '{exporter}' does not exist");
        }

        return exp;
    }
}