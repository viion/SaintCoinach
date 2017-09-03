﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

namespace SaintCoinach.Cmd {
    class ConsoleProgressReporter : IProgress<Ex.Relational.Update.UpdateProgress> {

        #region IProgress<UpdateProgress> Members

        public void Report(Ex.Relational.Update.UpdateProgress value) {
            Console.WriteLine(value);
        }

        #endregion
    }
    class Program {
        private static void Main(string[] args) {
            var dataPath = Properties.Settings.Default.DataPath;

            if (args.Length > 0) {
                dataPath = args[0];
                args = args.Skip(1).ToArray();
            }
            if (string.IsNullOrWhiteSpace(dataPath))
                dataPath = SearchForDataPaths().FirstOrDefault(p => System.IO.Directory.Exists(p));
            if (string.IsNullOrWhiteSpace(dataPath) || !System.IO.Directory.Exists(dataPath)) {
                Console.WriteLine("Need data!");
                return;
            }

            var realm = new ARealmReversed(dataPath, @"SaintCoinach.History.zip", Ex.Language.English, @"app_data.sqlite");
            realm.Packs.GetPack(new IO.PackIdentifier("exd", IO.PackIdentifier.DefaultExpansion, 0)).KeepInMemory = true;

            Console.WriteLine("Game version: {0}", realm.GameVersion);
            Console.WriteLine("Definition version: {0}", realm.DefinitionVersion);
            
            if (!realm.IsCurrentVersion) {
                Console.Write("Update is available, perform update (Y/n)? ");
                var updateQuery = Console.ReadLine();
                if (string.Equals("y", updateQuery, StringComparison.OrdinalIgnoreCase)) {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();
                    realm.Update(true, new ConsoleProgressReporter());
                    stopWatch.Stop();
                    Console.WriteLine(stopWatch.Elapsed);
                } else
                    Console.WriteLine("Skipping update");
            }

            var cns = new Tharga.Toolkit.Console.Command.Base.ClientConsole();
            var cmd = new RootCommand(cns);

            Setup(cmd, realm);

            (new CommandEngine(cmd)).Run(args);
        }

        static void Setup(RootCommand rootCmd, ARealmReversed realm) {
            var assembly = typeof(Program).Assembly;
            foreach (var t in assembly.GetTypes().Where(t => typeof(ActionCommandBase).IsAssignableFrom(t)))
            {
                var cons = t.GetConstructor(new[] { typeof(ARealmReversed) });
                rootCmd.RegisterCommand((ActionCommandBase)cons.Invoke(new[] { realm }));
            }
        }

        static string[] SearchForDataPaths() {
            const string gameFolder = "FINAL FANTASY XIV - A Realm Reborn";

            string programDir;
            if (Environment.Is64BitProcess)
                programDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            else
                programDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            return new string[] {
                System.IO.Path.Combine(programDir, "SquareEnix", gameFolder),
                System.IO.Path.Combine(@"D:\Games\SteamApps\common", gameFolder)
            };
        }
    }
}
