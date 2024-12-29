using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PizzaGospel
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
        public List<Window> Windows = new List<Window>();
        public static string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PizzaGospel\\");
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
                if (HasCompletedTXT)
                {
                    var S = File.ReadAllText(GamePath + "\\completed.txt");
                     
                    if(S != null)
                    Window.Log($"Got: {S}");
                }
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
            using (ZipArchive Archive = ZipFile.OpenRead(PathToZIP))
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
            if (Success) ChangeState(States.GotZIP);

        }

        public bool RetrieveData()
        {
            if (!File.Exists(ExtractPath + "config.txt")) return false;
            var ConfigTXT = File.ReadAllLines(ExtractPath + "config.txt");

            Name = "";
            Manifest = "2814056822728886841";
            bool GotAManifest = false;

            foreach (var Line in ConfigTXT)
            {
                if (Line.Split('=')[0] == "targetversion")
                {
                    Manifest = Line.Split('=')[1];
                    GotAManifest = true;
                }
                if (Line.Split('=')[0] == "name")
                    Name = Line.Split('=')[1].Replace("\"", "");
            }

            if(!GotAManifest) Window.Log("Warning! Missing manifest!");

            var TheoricName = Name != null && Name != "" ? Name : Path.GetFileNameWithoutExtension(ZIPPath);

            Name = TheoricName;

            Window.Log(CanLaunch ? $"Redrop: {TheoricName}" : $"Dropped: {TheoricName}");
            CanLaunch = true;
            PreviousInstall = false; // just in case
            return true;
        }


        public void LaunchGame()
        {
            Window.Log("Launching Game!");

            File.Delete(GamePath + "\\" + "steam_api64.dll");

            var PTProcess = new ProcessStartInfo()
            {
                FileName = Path.Combine(GamePath, "PizzaTower.exe"),
                Arguments = "-debug"
            };
            Process.Start(PTProcess);
            ChangeState(States.Launched);
            
            Window.TriggerClose();
        }

        public void ChangeState(States NewState) => State = NewState;

        public async void LaunchProcess()
        {

            Tools.ClearFolder(GamePath);
            ChangeState(States.Downloading);
            Window.Log("Downloading manifest...");
            ProcessStartInfo DownloaderInfo = new()
            {
                FileName = "DepotDownloader.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"-app 2231450 -depot 2231451 -username {Username} -password {Password} -manifest {Manifest} -dir {GamePath}"
            };

         
            Process DownloaderProcess = new();
            DownloaderProcess.StartInfo = DownloaderInfo;
            //DownloaderProcess.EnableRaisingEvents = true;
            var DepotConsole = new ConsoleWindow();
            bool Exited = false;

            DepotConsole.Show();
            DepotConsole.Left = Window.Left;
            DepotConsole.Top = Window.Top;
            DownloaderProcess.Start();

            void DepotCancel()
            {
                if (DownloaderProcess.HasExited && DownloaderProcess.ExitCode == 0) return;
                DownloaderProcess.Kill();
                DepotConsole.Dispatcher.Invoke(DepotConsole.Close);
                Window.Dispatcher.Invoke(Window.Show);
                Exited = true;
                return;
            }

            DepotConsole.Closed += delegate { DepotCancel(); };
            AppDomain.CurrentDomain.ProcessExit += delegate { DepotCancel(); };

            if (DownloaderProcess == null)
            {
                ChangeState(States.GotZIP);
                Window.LogsSTR = Window.LogsSTR.Replace("Downloading manifest...", "Failed to launch download!");
                Window.Logs.Text = Window.LogsSTR;
                return;
            }

            StringBuilder Loggy = new();

            Action<string> LoggyChange = DepotConsole.ChangeLogText;

            DownloaderProcess.OutputDataReceived += async (sender, args) => 
            {
                Loggy.AppendLine(args.Data);
                DepotConsole.Dispatcher.Invoke(LoggyChange, Loggy.ToString().Replace(GamePath, "-> Appdata\\Roaming\\PizzaGospel\\Game\\"));
                DepotConsole.Dispatcher.Invoke(DepotConsole.Logs.ScrollToEnd);

                if (args.Data == "The operation was canceled.")
                {
                    await Task.Delay(5000);
                    DepotCancel();
                }
            };

            DownloaderProcess.ErrorDataReceived += async (sender, args) => 
            {
                Loggy.AppendLine(args.Data);
                
                DepotConsole.Dispatcher.Invoke(LoggyChange, Loggy.ToString().Replace(GamePath, "-> Appdata\\Roaming\\PizzaGospel\\Game\\"));
                DepotConsole.Dispatcher.Invoke(DepotConsole.Logs.ScrollToEnd);

                if (args.Data == "The operation was canceled.")
                {
                    await Task.Delay(5000);
                    DepotCancel();
                }
            };

            DownloaderProcess.BeginOutputReadLine();
            DownloaderProcess.BeginErrorReadLine();
            Window.Hide();


            await DownloaderProcess.WaitForExitAsync();


            DepotConsole.Closed -= delegate { DepotCancel(); };
            AppDomain.CurrentDomain.ProcessExit -= delegate { DepotCancel(); };

            if (DownloaderProcess.ExitCode == 1 || Exited)
            {
                ChangeState(States.GotZIP);
                Window.LogsSTR = Window.LogsSTR.Replace("Downloading manifest...", "Failed download!");
                Window.Logs.Text = Window.LogsSTR;
                return;
            }

            await Task.Delay(DownloaderProcess.ExitCode == 1 ? 5000 : 3000);

            Window.Show();
            Window.Top = DepotConsole.Top;
            Window.Left = DepotConsole.Left;
            DepotConsole.Close();



            ChangeState(States.Patching);

            var PatchesList = Directory.GetFiles(ExtractPath).ToList().FindAll(x => Path.GetExtension(x) == ".xdelta");

            if (PatchesList.Count > 0) Window.Log("Patching...");

            foreach (var Patch in PatchesList)
            {
                ProcessStartInfo PatchingInfo = new()
                {
                    FileName = "xdelta.exe",
                    Arguments = $"-d -s \"{Path.Combine(GamePath, Path.GetFileName(Patch).Replace(".xdelta", ""))}\" \"{Patch}\" \"{Path.Combine(GamePath, "x" + Path.GetFileName(Patch).Replace(".xdelta", ""))}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                var PatchProcess = Process.Start(PatchingInfo);

                if (PatchProcess == null) break;

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
            if (PasteDir.Exists)
            {
                foreach (var File in PasteDir.GetFiles("*", SearchOption.AllDirectories))
                {
                    var FilePathFromPaste = File.FullName.Replace(PasteDir.FullName, "");
                    File.CopyTo(GamePath + "\\" + FilePathFromPaste, true);
                }
            }

            using (var SW = File.CreateText(GamePath + "\\completed.txt"))
                SW.WriteLine(Name);

            LaunchGame();

        }



        

    }
}
