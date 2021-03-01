using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models;

namespace Website.Server.Services
{
    public class PluginService
    {
        private readonly PluginsRepository pluginsRepository;

        public PluginService(PluginsRepository pluginsRepository)
        {
            this.pluginsRepository = pluginsRepository;
        }

        public byte[] ZipPlugin(PluginModel plugin)
        {
            using (var output = new MemoryStream())
            {
                using (var zip = new ZipOutputStream(output))
                {
                    zip.SetLevel(6);

                    var pluginEntry = new ZipEntry(plugin.FileName);
                    pluginEntry.DateTime = plugin.CreateDate;

                    zip.PutNextEntry(pluginEntry);
                    using (var ms = new MemoryStream(plugin.Data))
                    {
                        StreamUtils.Copy(ms, zip, new byte[4096]);
                    }
                    zip.CloseEntry();

                    foreach (var library in plugin.Libraries)
                    {
                        var entry = new ZipEntry(Path.Combine("Libraries", library.FileName));
                        entry.DateTime = plugin.CreateDate;

                        zip.PutNextEntry(entry);
                        using (var ms = new MemoryStream(library.Data))
                        {
                            StreamUtils.Copy(ms, zip, new byte[4096]);
                        }
                        zip.CloseEntry();
                    }

                    zip.Finish();

                    output.Position = 0;
                    return output.ToArray();
                }                
            }
        }
    }
}
