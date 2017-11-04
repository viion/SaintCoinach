using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console.Command.Base;

using SaintCoinach.Ex;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    public class SetFolderCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        /// <summary>
        /// Setup the command
        /// </summary>
        public SetFolderCommand(ARealmReversed realm)
            : base("setpath", "Set the folder for the FFXIV installation directory.") {
            _Realm = realm;
        }

        /// <summary>
        /// Obtain game sheets from the game data
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public override async Task<bool> InvokeAsync(string paramList) {
            Properties.Settings.Default.CustomPath = paramList.Trim();
            Properties.Settings.Default.Save();

            OutputInformation("Path set to: {0}", Properties.Settings.Default.CustomPath);
            OutputInformation("Deleting history.zip file ...");
            File.Delete(@"SaintCoinach.History.zip");

            OutputInformation("Restart the application for changes to take affect!");
            return true;
        }
    }
}