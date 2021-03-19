using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Client.Services
{
    public class ZIPService
    {
        public async Task<byte[]> ZipAsync(Dictionary<string, IEnumerable<IBrowserFile>> files)
        {
            using (var output = new MemoryStream())
            {
                using (var zip = new ZipOutputStream(output))
                {
                    zip.SetLevel(6);

                    foreach (var pair in files)
                    {
                        foreach (var file in pair.Value)
                        {
                            ZipEntry entry;
                            if (pair.Key == "/")
                                entry = new ZipEntry(file.Name);
                            else
                                entry = new ZipEntry(Path.Combine(pair.Key, file.Name));

                            entry.DateTime = DateTime.UtcNow;
                            zip.PutNextEntry(entry);
                            
                            byte[] buffer = new byte[file.Size];
                            await file.OpenReadStream(30 * 1024 * 1024).ReadAsync(buffer);

                            using (var ms = new MemoryStream(buffer))
                            {
                                StreamUtils.Copy(ms, zip, new byte[file.Size]);
                            }

                            zip.CloseEntry();
                        }
                    }

                    zip.Finish();

                    output.Position = 0;
                    return output.ToArray();
                }
            }
        }
    }
}
