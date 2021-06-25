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
            static Uri uri = new Uri("https://danslab.blob.core.windows.net/$web/HoloLensModelCoAuthorSKweb/model2.csv?sp=racwd&st=2021-06-24T21:05:29Z&se=2022-06-25T05:05:29Z&spr=https&sv=2020-02-10&sr=b&sig=rVUT7VaQoe%2FKDte3hHgGEBrOKpKgnBBfgqtZ69%2FnTIw%3D");
            static int autoupdateinterval = 5; //in seconds
            static Pose logPose = new Pose(new Vec3(0.4f, 0.2f, -0.4f), Quat.LookDir(-1, 0, 1));
            static List<string> logList = new List<string>();

            List<Vec3> meshPoints;
            List<Pose> meshPoses;
            List<Model> meshModels;

            bool autoupdateflag = false;

            public void GetRunning()
            {
                DateTime dtNow, dtLast = DateTime.UtcNow;

                meshPoints = new List<Vec3>();
                meshPoses = new List<Pose>();
                meshModels = new List<Model>();

                GrabMeshData();

                SKSettings settings = new SKSettings
                {
                    appName = "HoloLensModelCoAuthorSK",
                    assetsFolder = "Assets",
                };
                if (!SK.Initialize(settings))
                    Environment.Exit(1);

                Pose windowPose = new Pose(new Vec3(0.3f, 0, -0.3f), Quat.LookDir(-1, 0, 1));

                foreach (var v in meshPoints)
                {
                    meshPoses.Add(new Pose(v, Quat.Identity));
                    meshModels.Add(Model.FromMesh(Mesh.GenerateSphere(0.1f, 10), Default.MaterialUI));
                }

                while (SK.Step(() =>
                {
                    dtNow = DateTime.UtcNow;
                    Hand hand = Input.Hand(Handed.Right);
                    autoupdateflag = ((TimeSpan)(dtNow - dtLast)).TotalSeconds >= autoupdateinterval && !hand.IsPinched;
                    UI.WindowBegin("Settings", ref windowPose, new Vec2(20, 0) * U.cm);
                    if (UI.Button("Update") || autoupdateflag)
                    {
                        dtLast = DateTime.UtcNow;
                        GrabMeshData();
                        foreach (var v in meshPoints)
                        {
                            meshPoses.Add(new Pose(v, Quat.Identity));
                            meshModels.Add(Model.FromMesh(Mesh.GenerateSphere(0.1f, 10), Default.MaterialUI));
                        }
                    }
                    if (UI.Button("Save"))
                        PushMeshData();

                    UI.WindowEnd();

                    LogWindow();

                    if (meshPoints.Count > 0)
                        for (int i = 0; i < meshPoses.Count; i++)
                        {
                            Pose p = meshPoses[i];
                            Model m = meshModels[i];
                            if (UI.Handle("Cube" + i, ref p, m.Bounds))
                                if (hand.IsJustUnpinched)
                                    UpdatePointsFromMesh(i, p.position);                                
                            m.Draw(p.ToMatrix());
                            meshPoses[i] = p;
                        }
                }));
                SK.Shutdown();
            }

            private void LogWindow()
            {
                UI.WindowBegin("Log", ref logPose, new Vec2(40, 0) * U.cm);
                for (int i = 0; i < logList.Count; i++)
                    UI.Label(logList[i], false);
                UI.WindowEnd();
            }

            private void OnLog(LogLevel level, string text)
            {
                if (logList.Count > 10)
                    logList.RemoveAt(logList.Count - 1);
                logList.Insert(0, text.Length < 100 ? text : text.Substring(0, 100) + "...");
            }

            private void UpdatePointsFromMesh(int i, Vec3 v)
            {
                OnLog(LogLevel.Info, "updating [" + i + "] from (" + meshPoints[i] + ") to (" + v.ToString() + "]");
                meshPoints[i] = v;
                PushMeshData();
            }

            public void GrabMeshData()
            {
                meshPoints = new List<Vec3>();
                meshPoses = new List<Pose>();
                meshModels = new List<Model>();

                WebRequest request = WebRequest.Create(uri);
                WebResponse response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        meshPoints.Add(new Vec3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
                    }
                }
            }

            public void PushMeshData()
            {
                //TODO: implement Storage Account saving, per docs here: https://docs.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.upload?view=azure-dotnet#Azure_Storage_Blobs_BlobClient_Upload_System_String_Azure_Storage_Blobs_Models_BlobUploadOptions_System_Threading_CancellationToken_
                //      until then, the below is a quick/hacky way to get it up there
                string csvOut = "";
                for (int i = 0; i < meshPoints.Count; i++)
                    csvOut += meshPoints[i].x + "," + meshPoints[i].y + "," + meshPoints[i].z + (i == meshPoints.Count - 1 ? "" : "\r\n");
                OnLog(LogLevel.Info, csvOut);
                WebClient wc = new WebClient();
                wc.Headers.Add("x-ms-blob-type", "BlockBlob");
                wc.UploadData(uri, "PUT", System.Text.Encoding.ASCII.GetBytes(csvOut));
            }
        }
    }
}