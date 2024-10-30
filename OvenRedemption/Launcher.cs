using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OvenRedemption
{

    public class Launcher
    {
        public enum States
        {
            Idle = 0,
            GotZIP = 25,
            Downloading = 50,
            Patching = 90,
            Launched = 100,
        }

        public static Launcher Instance { get; private set; }
        public static MainWindow Window => MainWindow.Instance;
        public static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OvenRedemption\\");
        public static string ExtractPath => Path.Combine(DataPath + "Extract\\");
        public static string GamePath => Path.Combine(DataPath + "Game\\");

        public States State = States.Idle;

        public string Manifest = "";
        public string Name = "";

        public bool CanLaunch = false;
        public string Username = "";
        public string Password = "";
        public bool PreviousInstall = false;
        public string ZIPPath = "";

        public Launcher() 
        {
            Instance = this;
            SetupFolders();
            var GamePathDir = new DirectoryInfo(GamePath);
            if (GamePathDir.Exists)
            {
                var HasEXE = GamePathDir.GetFiles().ToList().Any(x => Path.GetFileName(x.FullName) == "PizzaTower.exe");
                var HasCompletedTXT = GamePathDir.GetFiles().ToList().Any(x => Path.GetFileName(x.FullName) == "completed.txt");
                PreviousInstall = HasEXE && HasCompletedTXT;
            }
        }


        public void SetupFolders() => Directory.CreateDirectory(DataPath);
        
        public void ClearEverything()
        {
            SetupFolders();
            Tools.ClearFolder(ExtractPath);
            Tools.ClearFolder(GamePath);
        }

        public bool ExtractDroppedZIP(string PathToZIP)
        {
            using(ZipArchive Archive = ZipFile.OpenRead(PathToZIP))
            {
                if (!Archive.Entries.Any(x => x.Name == "config.txt"))
                {
                    Window.Log("Failed! Missing config.txt!");
                    ChangeState(States.Idle);
                    return false;
                }
            }

            Tools.ClearFolder(ExtractPath);
            ZipFile.ExtractToDirectory(PathToZIP, ExtractPath);
            Window.ClearButton.IsEnabled = false;
            Window.ClearButton.Visibility = Visibility.Hidden;
            return true;
        }

        public void CheckDroppedFile(string FilePath, Action<bool> OnResult)
        {
            ZIPPath = FilePath;
            bool Extraction = ExtractDroppedZIP(FilePath);
            bool DataCollection = RetrieveData();

            var Success = Extraction && DataCollection;

            OnResult(Success);
            if(Success) ChangeState(States.GotZIP);

        }

        public bool RetrieveData()
        {
            if (!File.Exists(ExtractPath + "config.txt")) return false;
            var ConfigTXT = File.ReadAllLines(ExtractPath + "config.txt");

            foreach (var Line in ConfigTXT)
            {
                if (Line.Split('=')[0] == "targetversion")
                    Manifest = Line.Split('=')[1];
                if (Line.Split('=')[0] == "name")
                    Name = Line.Split('=')[1].Replace("\"", "");
            }

            var TheoricName = Name != null && Name != "" ? Name : Path.GetFileNameWithoutExtension(ZIPPath);

            Window.Log(CanLaunch ? $"Redrop: {TheoricName}" : $"Dropped: {TheoricName}");
            CanLaunch = true;
            PreviousInstall = false; // just in case
            return true;
        }


        public void LaunchGame()
        {
            Window.Log("Launching Game!");

            File.Delete(GamePath + "\\" + "steam_api64.dll");

            var PTProcess = new Process();
            PTProcess.StartInfo.FileName = Path.Combine(GamePath, "PizzaTower.exe");
            PTProcess.StartInfo.Arguments = "-debug";
            PTProcess.Start();
            ChangeState(States.Launched);
            Window.TriggerClose();
        }

        public void ChangeState(States NewState) => State = NewState;
        

       

        public async void LaunchProcess()
        {
            Tools.ClearFolder(GamePath);
            ChangeState(States.Downloading);
            Window.Log("Downloading manifest...");
            var CMDProcess = new Process();
            CMDProcess.StartInfo.FileName = "DepotDownloader.exe";
            // CMDProcess.StartInfo.CreateNoWindow = true;
            CMDProcess.StartInfo.Arguments = $"-app 2231450 -depot 2231451 -username {Username} -password {Password} -manifest {Manifest} -dir {GamePath}";
            CMDProcess.Start();
            await CMDProcess.WaitForExitAsync();

            if (CMDProcess.ExitCode == 1)
            {
                ChangeState(States.GotZIP);
                Window.LogsSTR = Window.LogsSTR.Replace("Downloading manifest...", "Failed download!");
                Window.Logs.Text = Window.LogsSTR;
                return;
            }

            ChangeState(States.Patching);

            var PatchesList = Directory.GetFiles(ExtractPath).ToList().FindAll(x => Path.GetExtension(x) == ".xdelta");

            if(PatchesList.Count > 0) Window.Log("Patching..."); 

            foreach (var Patch in PatchesList)
            {
                var PatchProcess = new Process();
                PatchProcess.StartInfo.FileName = "xdelta.exe";
                PatchProcess.StartInfo.Arguments = $"-d -s \"{Path.Combine(GamePath, Path.GetFileName(Patch).Replace(".xdelta", ""))}\" \"{Patch}\" \"{Path.Combine(GamePath, "x" + Path.GetFileName(Patch).Replace(".xdelta", ""))}\"";
                PatchProcess.StartInfo.CreateNoWindow = true;
                PatchProcess.StartInfo.UseShellExecute = false;

                PatchProcess.Start();
                await PatchProcess.WaitForExitAsync();

                if (PatchProcess.ExitCode == 1)
                {
                    ChangeState(States.GotZIP);
                    Window.LogsSTR = Window.LogsSTR.Replace("Patching...", "Patch failed!");
                    Window.Logs.Text = Window.LogsSTR;
                    return;
                }

                var PatchedFile = File.ReadAllBytes(Path.Combine(GamePath, "x" + Path.GetFileName(Patch).Replace(".xdelta", "")));
                File.WriteAllBytes(Path.Combine(GamePath, Path.GetFileName(Patch).Replace(".xdelta", "")), PatchedFile);
                File.Delete(Path.Combine(GamePath, "x" + Path.GetFileName(Patch).Replace(".xdelta", "")));

            }
            var PasteDir = new DirectoryInfo(Path.Combine(ExtractPath, "Paste"));
            if(PasteDir.Exists)
            {
                foreach (var File in PasteDir.GetFiles("*", SearchOption.AllDirectories))
                {
                    var FilePathFromPaste = File.FullName.Replace(PasteDir.FullName, "");
                    File.CopyTo(GamePath + "\\" + FilePathFromPaste, true);
                }
            }

            File.Create(GamePath + "\\completed.txt");
            LaunchGame();
            
        }
        
    }
}
