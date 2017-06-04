/*
 * MIT License
 * 
 * Copyright (c) 2017 Michael VanOverbeek and ShiftOS devs
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#define DEVEL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShiftOS.Engine.Properties;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;

using ShiftOS.Objects;
using Discoursistency.Base.Models.Authentication;
using ShiftOS.Engine.Scripting;
using ShiftOS.Objects.ShiftFS;

namespace ShiftOS.Engine
{
    [Namespace("infobox", hide = true)]
    [RequiresUpgrade("desktop;wm_free_placement")]
    public static class InfoboxDebugCommands
    {

        [RequiresArgument("title")]
        [RequiresArgument("msg")]
        [Command("show")]
        public static bool ShowInfo(Dictionary<string, object> args)
        {
            Desktop.InvokeOnWorkerThread(new Action(() =>
            {
                Infobox.Show(args["title"].ToString(), args["msg"].ToString());
            }));
            return true;
        }

        [RequiresArgument("title")]
        [RequiresArgument("msg")]
        [Command("yesno")]
        public static bool ShowYesNo(Dictionary<string, object> args)
        {
            bool forwarding = TerminalBackend.IsForwardingConsoleWrites;
            var fGuid = TerminalBackend.ForwardGUID;
            Action<bool> callback = (result) =>
            {
                TerminalBackend.IsForwardingConsoleWrites = forwarding;
                TerminalBackend.ForwardGUID = (forwarding == true) ? fGuid : null;
                string resultFriendly = (result == true) ? "yes" : "no";
                Console.WriteLine($"{SaveSystem.CurrentUser.Username} says {resultFriendly}.");
                TerminalBackend.IsForwardingConsoleWrites = false;
            };
            Desktop.InvokeOnWorkerThread(new Action(() =>
            {
                Infobox.PromptYesNo(args["title"].ToString(), args["msg"].ToString(), callback);

            }));
            return true;
        }

        [RequiresArgument("title")]
        [RequiresArgument("msg")]
        [Command("text")]
        public static bool ShowText(Dictionary<string, object> args)
        {
            bool forwarding = TerminalBackend.IsForwardingConsoleWrites;
            var fGuid = TerminalBackend.ForwardGUID;
            Action<string> callback = (result) =>
            {
                TerminalBackend.IsForwardingConsoleWrites = forwarding;
                TerminalBackend.ForwardGUID = (forwarding == true) ? fGuid : null;
                Console.WriteLine($"{SaveSystem.CurrentUser.Username} says \"{result}\".");
                TerminalBackend.IsForwardingConsoleWrites = false;
            };
            Desktop.InvokeOnWorkerThread(new Action(() =>
            {
                Infobox.PromptText(args["title"].ToString(), args["msg"].ToString(), callback);
            }));
                return true;
        }

    }

    [Namespace("audio")]
    public static class AudioCommands
    {
        [Command("setvol", description = "Set the volume of the system audio to anywhere between 0 and 100.")]
        [RequiresArgument("value")]
        [RequiresUpgrade("audio_volume")]
        public static bool SetVolume(Dictionary<string,object> args)
        {
            int val = Convert.ToInt32(args["value"].ToString());
            float volume = (val / 100F);
            AudioManager.SetVolume(volume);
            return true;
        }
        [RequiresUpgrade("audio_volume")]
        [Command("mute", description = "Sets the volume of the system audio to 0")]
        public static bool MuteAudio()
        {
            AudioManager.SetVolume(0);
            return true;
        }
    }

    [RequiresUpgrade("mud_fundamentals")]
    [Namespace("mud")]
    public static class MUDCommands
    {
        [MultiplayerOnly]
        [Command("status")]
        public static bool Status()
        {
            ServerManager.PrintDiagnostics();
            return true;
        }

        [Command("connect")]
        public static bool Connect(Dictionary<string, object> args)
        {
            try
            {
                string ip = (args.ContainsKey("addr") == true) ? args["addr"] as string : "michaeltheshifter.me";
                int port = (args.ContainsKey("port") == true) ? Convert.ToInt32(args["port"] as string) : 13370;
                try
                {
                    ServerManager.Initiate(ip, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{ERROR}: " + ex.Message);
                }

                TerminalBackend.PrefixEnabled = false;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running script:" + ex);
                return false;
            }
        }

        [Command("reconnect")]
        [RequiresUpgrade("hacker101_deadaccts")]
        public static bool Reconnect()
        {
            Console.WriteLine("--reconnecting to multi-user domain...");
            KernelWatchdog.MudConnected = true;
            Console.WriteLine("--done.");
            return true;
        }

        [MultiplayerOnly]
        [Command("disconnect")]
        [RequiresUpgrade("hacker101_deadaccts")]
        public static bool Disconnect()
        {
            Console.WriteLine("--connection to multi-user domain severed...");
            KernelWatchdog.MudConnected = false;
            return true;
        }

        [MultiplayerOnly]
        [Command("sendmsg")]
        [KernelMode]
        [RequiresUpgrade("hacker101_deadaccts")]
        [RequiresArgument("header")]
        [RequiresArgument("body")]
        public static bool SendMessage(Dictionary<string, object> args)
        {
            ServerManager.SendMessage(args["header"].ToString(), args["body"].ToString());
            return true;
        }
    }

    [TutorialLock]
    [Namespace("trm")]
    public static class TerminalCommands
    {
        [Command("clear")]
        public static bool Clear()
        {
            AppearanceManager.ConsoleOut.Clear();
            return true;
        }

        [Command("echo")]
        [RequiresArgument("text")]
        public static bool Echo(Dictionary<string, object> args)
        {
            Console.WriteLine(args["text"]);
            return true;
        }
    }

#if DEVEL
    internal class Rock : Exception
    {
        internal Rock() : base("Someone threw a rock at the window, and the Terminal shattered.")
        {

        }
    }
    
    [MultiplayerOnly]
    [Namespace("dev")]
    public static class ShiftOSDevCommands
    {
        [Command("buy")]
        public static bool UnlockUpgrade(Dictionary<string, object> args)
        {
            string upg = args["id"].ToString();
            try
            {
                SaveSystem.CurrentSave.Upgrades[upg] = true;
                Shiftorium.InvokeUpgradeInstalled();
                SaveSystem.SaveGame();
            }
            catch
            {
                Console.WriteLine("Upgrade not found.");
            }
            return true;
        }

        [Command("rock", description = "A little surprise for unstable builds...")]
        public static bool ThrowASandwichingRock()
        {
            Infobox.Show("He who lives in a glass house shouldn't throw stones...", new Rock().Message);
            return false;
        }


        [Command("unbuy")]
        [RequiresArgument("upgrade")]
        public static bool UnbuyUpgrade(Dictionary<string, object> args)
        {
            try
            {
                SaveSystem.CurrentSave.Upgrades[args["upgrade"].ToString()] = false;
                SaveSystem.SaveGame();
                Desktop.PopulateAppLauncher();
                Desktop.CurrentDesktop.SetupDesktop();
            }
            catch
            {
                Console.WriteLine("Upgrade not found.");
            }
            return true;
        }

        [Command("getallupgrades")]
        public static bool GetAllUpgrades()
        {
            Console.WriteLine(JsonConvert.SerializeObject(SaveSystem.CurrentSave.Upgrades, Formatting.Indented));
            return true;
        }

        [Command("multarg")]
        [RequiresArgument("id")]
        [RequiresArgument("name")]
        [RequiresArgument("type")]
        public static bool MultArg(Dictionary<string, object> args)
        {
            Console.WriteLine("Success! "+args.ToString());
            return true;
        }

        [Command("restart")]
        public static bool Restart()
        {
            SaveSystem.CurrentSave.Upgrades = new Dictionary<string, bool>();
            SaveSystem.CurrentSave.Codepoints = 0;
            SaveSystem.CurrentSave.StoriesExperienced.Clear();
            SaveSystem.CurrentSave.StoriesExperienced.Add("mud_fundamentals");
            SaveSystem.SaveGame();
            Shiftorium.InvokeUpgradeInstalled();
            return true;
        }

        [Command("freecp")]
        public static bool FreeCodepoints(Dictionary<string, object> args)
        {
            if (args.ContainsKey("amount"))
                try
                {
                    ulong codepointsToAdd = Convert.ToUInt64(args["amount"].ToString());
                    SaveSystem.CurrentSave.Codepoints += codepointsToAdd;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{ERROR}: " + ex.Message);
                    return true;
                }

            SaveSystem.CurrentSave.Codepoints += 1000;
            return true;
        }

        [Command("unlockeverything")]
        public static bool UnlockAllUpgrades()
        {
            foreach (var upg in Shiftorium.GetDefaults())
            {
                if (!SaveSystem.CurrentSave.Upgrades.ContainsKey(upg.ID))
                    SaveSystem.CurrentSave.Upgrades.Add(upg.ID, true);
                else
                    SaveSystem.CurrentSave.Upgrades[upg.ID] = true;
            }
            Shiftorium.InvokeUpgradeInstalled();
            SkinEngine.LoadSkin();
            return true;
        }

        [Command("info")]
        public static bool DevInformation()
        {
            Console.WriteLine("{SHIFTOS_PLUS_MOTTO}");
            Console.WriteLine("{SHIFTOS_VERSION_INFO}" + Assembly.GetExecutingAssembly().GetName().Version);
            return true;
        }
        [Command("pullfile")]
        public static bool PullFile(Dictionary<string, object> args)
        {
            if (args.ContainsKey("physical") && args.ContainsKey("virtual"))
            {
                string file = (string)args["physical"];
                string dest = (string)args["virtual"];
                if (System.IO.File.Exists(file))
                {
                    Console.WriteLine("Pulling physical file to virtual drive...");
                    byte[] filebytes = System.IO.File.ReadAllBytes(file);
                    ShiftOS.Objects.ShiftFS.Utils.WriteAllBytes(dest, filebytes);
                }
                else
                {
                    Console.WriteLine("The specified file does not exist on the physical drive.");
                }
            }
            else
            {
                Console.WriteLine("You must supply a physical path.");
            }
            return true;
        }
        [Command("crash")]
        public static bool CrashInstantly()
        {
            CrashHandler.Start(new Exception("ShiftOS was sent a command to forcefully crash."));
            return true;
        }
    }
#endif

    [Namespace("sos")]
    public static class ShiftOSCommands
    {

        [Command("setsfxenabled", description = "Set whether or not sound effects are enabled in the system.")]
        [RequiresArgument("value")]
        public static bool SetSfxEnabled(Dictionary<string, object> args)
        {
            try
            {
                bool value = Convert.ToBoolean(args["value"].ToString());
                SaveSystem.CurrentSave.SoundEnabled = value;
                SaveSystem.SaveGame();
            }
            catch
            {
                Console.WriteLine("Error: Value must be either true or false.");
            }
            return true;
        }



        [Command("setmusicenabled", description = "Set whether or not music is enabled in the system.")]
        [RequiresArgument("value")]
        public static bool SetMusicEnabled(Dictionary<string, object> args)
        {
            try
            {
                bool value = Convert.ToBoolean(args["value"].ToString());
                SaveSystem.CurrentSave.MusicEnabled = value;
                SaveSystem.SaveGame();
            }
            catch
            {
                Console.WriteLine("Error: Value must be either true or false.");
            }
            return true;
        }

        

        [Command("setsfxvolume", description ="Set the system sound volume to a value between 1 and 100.")]
        [RequiresArgument("value")]
        public static bool SetSfxVolume(Dictionary<string, object> args)
        {
            int value = int.Parse(args["value"].ToString());
            if(value >= 0 && value <= 100)
            {
                SaveSystem.CurrentSave.MusicVolume = value;
                SaveSystem.SaveGame();
            }
            else
            {
                Console.WriteLine("Volume must be between 0 and 100!");
            }
            return true;
        }

        [RemoteLock]
        [Command("shutdown")]
        public static bool Shutdown()
        {
            TerminalBackend.InvokeCommand("sos.save");
            AppearanceManager.Exit();
            return true;
        }

        [Command("lang", "{COMMAND_SOS_LANG_USAGE}", "{COMMAND_SOS_LANG_DESCRIPTION}")]
        [RequiresArgument("language")]
        public static bool SetLanguage(Dictionary<string, object> userArgs)
        {
            try
            {
                string lang = "";

                if (userArgs.ContainsKey("language"))
                    lang = (string)userArgs["language"];
                else
                    throw new Exception("You must specify a valid 'language' value.");

                if (Localization.GetAllLanguages().Contains(lang))
                {
                    SaveSystem.CurrentSave.Language = lang;
                    SaveSystem.SaveGame();
                    Console.WriteLine("{LANGUAGE_CHANGED}");
                    return true;
                }

                throw new Exception($"Couldn't find language with ID: {lang}");
            }
            catch
            {
                return false;
            }
        }

        [Command("help", "{COMMAND_HELP_USAGE", "{COMMAND_HELP_DESCRIPTION}")]
        public static bool Help(Dictionary<string, object> args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Retrieving help data.");

            if (args.ContainsKey("ns"))
            {
                string ns = args["ns"].ToString();
                //First let's check for a command that has this namespace.
                var cmdtest = TerminalBackend.Commands.FirstOrDefault(x => x.NamespaceInfo.name == ns);
                if (cmdtest == null) //Namespace not found.
                    sb.AppendLine("Error retrieving help for namespace \"" + ns + "\". Namespace not found.");
                else
                {
                    //Now do the actual scan.
                    sb.AppendLine("Namespace: " + ns);
                    foreach(var cmd in TerminalBackend.Commands.Where(x => x.NamespaceInfo.name == ns))
                    {
                        string str = cmd.ToString();
                        str = str.Replace(str.Substring(str.LastIndexOf("|")), "");
                        sb.AppendLine(str);
                    }
                }
            }
            else
            {
                
                //print all unique namespaces.
                foreach(var n in TerminalBackend.Commands.Select(x => x.NamespaceInfo.name).Distinct())
                {
                    sb.AppendLine("sos.help{ns:\"" + n + "\"}");
                }
            }

            Console.WriteLine(sb.ToString());

            return true;
        }

        [MultiplayerOnly]
        [Command("save")]
        public static bool Save()
        {
            SaveSystem.SaveGame();
            return true;
        }

        [MultiplayerOnly]
        [Command("status")]
        public static bool Status()
        {
            string status = $@"ShiftOS version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}

Codepoints:       {SaveSystem.CurrentSave.Codepoints}
Upgrades:         {SaveSystem.CurrentSave.CountUpgrades()} installed,
                  {Shiftorium.GetAvailable().Length} available

";

            Console.WriteLine(status);
            Console.WriteLine("Objectives:");
            try
            {
                if (Story.CurrentObjectives.Count > 0)
                {
                    foreach (var obj in Story.CurrentObjectives)
                    {
                        Console.WriteLine(obj.Name);
                        Console.WriteLine("-------------------------------");
                        Console.WriteLine();
                        Console.WriteLine(obj.Description);
                    }
                }
                else
                {
                    Console.WriteLine(" - No objectives are running. Check back later!");
                }
            }
            catch
            {
                Console.WriteLine(" - No objectives are running. Check back later!");
            }
            return true;
        }
    }

    [MultiplayerOnly]
    [Namespace("shiftorium")]
    public static class ShiftoriumCommands
    {
        [Command("buy")]
        [RequiresArgument("upgrade")]
        public static bool BuyUpgrade(Dictionary<string, object> userArgs)
        {
            try
            {
                string upgrade = "";

                if (userArgs.ContainsKey("upgrade"))
                    upgrade = (string)userArgs["upgrade"];
                else
                    throw new Exception("You must specify a valid 'upgrade' value.");

                foreach (var upg in Shiftorium.GetAvailable())
                {
                    if (upg.ID == upgrade)
                    {
                        Shiftorium.Buy(upgrade, upg.Cost);
                        return true;
                    }
                }

                throw new Exception($"Couldn't find upgrade with ID: {upgrade}");
            }
            catch
            {
                return false;
            }
        }

        [RequiresUpgrade("shiftorium_bulk_buy")]
        [Command("bulkbuy")]
        [RequiresArgument("upgrades")]
        public static bool BuyBulk(Dictionary<string, object> args)
        {
            if (args.ContainsKey("upgrades"))
            {
                string[] upgrade_list = (args["upgrades"] as string).Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var upg in upgrade_list)
                {
                    var dict = new Dictionary<string, object>();
                    dict.Add("upgrade", upg);
                    BuyUpgrade(dict);
                }
            }
            else
            {
                throw new Exception("Please specify a list of upgrades in the 'upgrades' argument. Each upgrade is separated by a comma.");
            }
            return true;
        }


        [Command("info")]
        public static bool ViewInfo(Dictionary<string, object> userArgs)
        {
            try
            {
                string upgrade = "";

                if (userArgs.ContainsKey("upgrade"))
                    upgrade = (string)userArgs["upgrade"];
                else
                    throw new Exception("You must specify a valid 'upgrade' value.");

                foreach (var upg in Shiftorium.GetDefaults())
                {
                    if (upg.ID == upgrade)
                    {
                        Console.WriteLine($@"Information for {upgrade}:

{upg.Category}: {upg.Name} - {upg.Cost} Codepoints
------------------------------------------------------

{upg.Description}

To buy this upgrade, run:
shiftorium.buy{{upgrade:""{upg.ID}""}}");
                        return true;
                    }
                }

                throw new Exception($"Couldn't find upgrade with ID: {upgrade}");
            }
            catch
            {
                return false;
            }
        }

        [Command("categories")]
        public static bool ListCategories()
        {
            foreach(var cat in Shiftorium.GetCategories())
            {
                Console.WriteLine($"{cat} - {Shiftorium.GetAvailable().Where(x=>x.Category==cat).Count()} upgrades");
            }
            return true;
        }

        [Command("list")]
        public static bool ListAll(Dictionary<string, object> args)
        {
            try
            {
                bool showOnlyInCategory = false;

                string cat = "Other";

                if (args.ContainsKey("cat"))
                {
                    showOnlyInCategory = true;
                    cat = args["cat"].ToString();
                }

                Dictionary<string, ulong> upgrades = new Dictionary<string, ulong>();
                int maxLength = 5;

                IEnumerable<ShiftoriumUpgrade> upglist = Shiftorium.GetAvailable();
                if (showOnlyInCategory)
                {
                    if (Shiftorium.IsCategoryEmptied(cat))
                    {
                        ConsoleEx.Bold = true;
                        ConsoleEx.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Shiftorium Query Error");
                        Console.WriteLine();
                        ConsoleEx.Bold = false;
                        ConsoleEx.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Either there are no upgrades in the category \"" + cat + "\" or the category was not found.");
                        return true;
                    }
                    upglist = Shiftorium.GetAvailable().Where(x => x.Category == cat);
                }


                if(upglist.Count() == 0)
                {
                    ConsoleEx.Bold = true;
                    ConsoleEx.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No upgrades available!");
                    Console.WriteLine();
                    ConsoleEx.Bold = false;
                    ConsoleEx.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("You have installed all available upgrades for your system. Please check back later for more.");
                    return true;

                }
                foreach (var upg in upglist)
                {
                    if (upg.ID.Length > maxLength)
                    {
                        maxLength = upg.ID.Length;
                    }

                    upgrades.Add(upg.ID, upg.Cost);
                }

                Console.WriteLine("ID".PadRight((maxLength + 5) - 2) + "Cost (Codepoints)");

                foreach (var upg in upgrades)
                {
                    Console.WriteLine(upg.Key.PadRight((maxLength + 5) - upg.Key.Length) + "  " + upg.Value.ToString());
                }
                return true;
            }
            catch (Exception e)
            {
                CrashHandler.Start(e);
                return false;
            }
        }
    }

    [Namespace("win")]
    public static class WindowCommands
    {



        [RemoteLock]
        [Command("list")]
        public static bool List()
        {
            Console.WriteLine("Window ID\tName");
            foreach (var app in AppearanceManager.OpenForms)
            {
                //Windows are displayed the order in which they were opened.
                Console.WriteLine($"{AppearanceManager.OpenForms.IndexOf(app)}\t{app.Text}");
            }
            return true;
        }

        [RemoteLock]
        [Command("open")]
        public static bool Open(Dictionary<string, object> args)
        {
            try
            {
                if (args.ContainsKey("app"))
                {
                    var app = args["app"] as string;
                    //ANNND now we start reflecting...
                    foreach (var asmExec in System.IO.Directory.GetFiles(Environment.CurrentDirectory))
                    {
                        if (asmExec.EndsWith(".exe") || asmExec.EndsWith(".dll"))
                        {
                            var asm = Assembly.LoadFile(asmExec);
                            try
                            {
                                foreach (var type in asm.GetTypes())
                                {
                                    if (type.BaseType == typeof(UserControl))
                                    {
                                        foreach (var attr in type.GetCustomAttributes(false))
                                        {
                                            if (attr is WinOpenAttribute)
                                            {
                                                if (app == (attr as WinOpenAttribute).ID)
                                                {
                                                    if (SaveSystem.CurrentSave.Upgrades.ContainsKey(app))
                                                    {
                                                        if (Shiftorium.UpgradeInstalled(app))
                                                        {
                                                            IShiftOSWindow frm = Activator.CreateInstance(type) as IShiftOSWindow;
                                                            AppearanceManager.SetupWindow(frm);
                                                            return true;
                                                        }
                                                        else
                                                        {
                                                            throw new Exception($"{app} was not found on your system! Try looking in the shiftorium...");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        IShiftOSWindow frm = Activator.CreateInstance(type) as IShiftOSWindow;
                                                        AppearanceManager.SetupWindow(frm);
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }

                        }
                    }

                }
                else
                {
                    foreach (var asmExec in System.IO.Directory.GetFiles(Environment.CurrentDirectory))
                    {
                        if (asmExec.EndsWith(".exe") || asmExec.EndsWith(".dll"))
                        {
                            try
                            {
                                var asm = Assembly.LoadFile(asmExec);

                                foreach (var type in asm.GetTypes())
                                {
                                    if (type.GetInterfaces().Contains(typeof(IShiftOSWindow)))
                                    {
                                        foreach (var attr in type.GetCustomAttributes(false))
                                        {
                                            if (attr is WinOpenAttribute)
                                            {
                                                if (Shiftorium.UpgradeAttributesUnlocked(type))
                                                {
                                                    Console.WriteLine("win.open{app:\"" + (attr as WinOpenAttribute).ID + "\"}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }


                    return true;
                }
                Console.WriteLine("Couldn't find the specified app on your system.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error running script:" + ex);
                return false;
            }
        }

        [RemoteLock]
        [Command("close", usage = "{win:integer32}", description ="Closes the specified window.")]
        [RequiresArgument("win")]
        [RequiresUpgrade("close_command")]
        public static bool CloseWindow(Dictionary<string, object> args)
        {
            int winNum = -1;
            if (args.ContainsKey("win"))
                winNum = Convert.ToInt32(args["win"].ToString());
            string err = null;

            if (winNum < 0 || winNum >= AppearanceManager.OpenForms.Count)
                err = "The window number must be between 0 and " + (AppearanceManager.OpenForms.Count - 1).ToString() + ".";

            if (string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"Closing {AppearanceManager.OpenForms[winNum].Text}...");
                AppearanceManager.Close(AppearanceManager.OpenForms[winNum].ParentWindow);
            }
            else
            {
                Console.WriteLine(err);
            }

            return true;
        }

    }
}
