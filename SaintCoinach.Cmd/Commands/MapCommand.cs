using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Command;
using Tharga.Toolkit.Console.Command.Base;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands
{
    public class MapCommand : ActionCommandBase
    {
        private ARealmReversed _Realm;

        public MapCommand(ARealmReversed realm)
            : base("maps", "Export all map images.")
        {
            _Realm = realm;
        }

        /// <summary>
        /// Invoice extract
        /// </summary>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public override async Task<bool> InvokeAsync(string paramList)
        {
            var format = ImageFormat.Jpeg;
            
            if (!string.IsNullOrEmpty(paramList))
            {
                var parameters = paramList.Split(' ');
                if (parameters.Contains("jpg"))
                    format = ImageFormat.Jpeg;
                else if (parameters.Contains("png"))
                    format = ImageFormat.Png;
                else
                    OutputError("Invalid map format " + paramList);
                }

            var c = 0;
            var allMaps = _Realm.GameData.GetSheet<SaintCoinach.Xiv.Map>();

            foreach (var map in allMaps)
            {
                var img = map.MediumImage;
                if (img == null)
                    continue;

                var outPathSb = new StringBuilder("ui/map/");

                // format: map/map.id.extension
                outPathSb.AppendFormat("{0}/{1}{2}", 
                    map.Id.ToString().Split('/')[0], 
                    map.Id.ToString().Replace('/', '.'), 
                    FormatToExtension(format)
                );

                var outFile = new FileInfo(Path.Combine(_Realm.GameVersion, outPathSb.ToString()));

                if (!outFile.Directory.Exists)
                    outFile.Directory.Create();

                OutputInformation("Saving map: {0}", outPathSb);
                img.Save(outFile.FullName, format);
                ++c;
            }

            OutputInformation("{0} maps saved", c);

            return true;
        }

        /// <summary>
        /// Format map file to an extension
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        static string FormatToExtension(ImageFormat format)
        {
            if (format == ImageFormat.Png)
                return ".png";
            if (format == ImageFormat.Jpeg)
                return ".jpg";

            throw new NotImplementedException();
        }
    }
}
