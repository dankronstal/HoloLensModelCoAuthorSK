using StereoKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace HoloLensModelCoAuthorSK
{
    class Program
    {
        static void Main(string[] args)
        {
            Runner r = new Runner();
            r.GetRunning();
        }

        public class Runner
        {
            List<Vec3> meshPoints;
            List<Pose> meshPoses;
            List<Model> meshModels;

            public void GetRunning()
            {
                meshPoints = new List<Vec3>();
                meshPoses = new List<Pose>();
                meshModels = new List<Model>();

                GrabMeshData();

                // Initialize StereoKit
                SKSettings settings = new SKSettings
                {
                    appName = "HoloLensModelCoAuthorSK",
                    assetsFolder = "Assets",
                };
                if (!SK.Initialize(settings))
                    Environment.Exit(1);

                Pose windowPose = new Pose(new Vec3(0.3f, 0, -0.3f), Quat.LookDir(-1, 0, 1));

                // Update mesh
                foreach (var v in meshPoints)
                {
                    meshPoses.Add(new Pose(v, Quat.Identity));
                    meshModels.Add(Model.FromMesh(Mesh.GenerateSphere(0.1f, 10), Default.MaterialUI));
                }

                Hand hand = Input.Hand(Handed.Right);

                // Core application loop
                while (SK.Step(() =>
                {                    
                    UI.WindowBegin("Settings", ref windowPose, new Vec2(20, 0) * U.cm);
                    if (UI.Button("Update"))
                    {
                        GrabMeshData();
                        // Update mesh
                        foreach (var v in meshPoints)
                        {
                            meshPoses.Add(new Pose(v, Quat.Identity));
                            meshModels.Add(Model.FromMesh(Mesh.GenerateSphere(0.1f, 10), Default.MaterialUI));
                        }
                    }
                    if (UI.Button("Save"))
                    {
                        PushMeshData();
                    }
                    UI.WindowEnd();

                    if (meshPoints.Count > 0)
                        for (int i = 0; i < meshPoses.Count; i++)
                        {
                            Pose p = meshPoses[i];
                            Model m = meshModels[i];
                            UI.Handle("Cube" + i, ref p, m.Bounds);
                            //TODO: add event for move completed: if(UI.Handle(...)) if(hand.IsJustUnpinched)  <do some stuff, like update meshPoint[i] with new location and autosave>
                            m.Draw(p.ToMatrix());
                            meshPoses[i] = p;
                        }

                    if (hand.IsJustUnpinched)
                        PushMeshData();
                })) ;
                SK.Shutdown();
            }
            public void GrabMeshData()
            {
                meshPoints = new List<Vec3>();
                meshPoses = new List<Pose>();
                meshModels = new List<Model>();

                WebRequest request = WebRequest.Create("https://danslab.blob.core.windows.net/$web/model.csv");
                WebResponse response = request.GetResponse();
                Console.WriteLine("");
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        meshPoints.Add(new Vec3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
                    }
                }

                UpdateMesh(meshPoints);
            }

            public void PushMeshData()
            {
                Console.WriteLine("PUSHING!!!!!");
                //TODO: implement Storage Account saving, per docs here: https://docs.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.upload?view=azure-dotnet#Azure_Storage_Blobs_BlobClient_Upload_System_String_Azure_Storage_Blobs_Models_BlobUploadOptions_System_Threading_CancellationToken_
            }

            public void UpdateMesh(List<Vec3> points)
            {
                //TODO: Move update code in here
            }
        }
    }
}