using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace mc_res_downloader
{

    public class Minedex
    {
        public static readonly string indexUrlPlaceholder = "https://raw.githubusercontent.com/MultiMC/meta-multimc/master/net.minecraft/{0}.json";
        static string mc_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
        public static string MC_DIR {
            get => mc_dir;
            set => mc_dir = Path.TrimEndingDirectorySeparator(value);
        }
        /// <summary>
        /// Verifica que exista la ruta relativa a MC_DIR, si no existe, la crea y retorna la ruta completa
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ensureDir(string path, string? file = null)
        {
            path = path.Replace("/", "\\");
            path = path.StartsWith("\\") ? path.Substring(1) : path;
            path = mc_dir + "\\" + path;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            return path + (string.IsNullOrEmpty(file) ? "" : file);
        }

        [JsonPropertyName("objects")]
        public Dictionary<string, MineObject> objects { get; set; } 

        public Minedex()
        {
            objects = new();
        }
    }

    public class MineObject
    {
        [JsonPropertyName("hash")]
        public string hash { get; set; }
        [JsonPropertyName("size")]
        public int size { get; set; }
        public string? url
        {
            get
            {
                return "https://resources.download.minecraft.net/" + hash.Substring(0, 2) + "/" + hash;
            }
        }

        public MineObject()
        {
            hash = string.Empty;
            size = 0;
        }
    }
    public class MineIndexProf
    {
        [JsonPropertyName("assetIndex")]
        public MineAssetIndex assetIndex { get; set; }
        public MineIndexProf()
        {
            assetIndex= new MineAssetIndex();
        }
    }
    public class MineAssetIndex
    {
        public string id { get; set; }
        public string sha1 { get; set; }
        public int size { get; set; }
        public int totalSize { get; set; }
        public string url { get; set; }

        public MineAssetIndex()
        {
            id = "";
            sha1 = "";
            size = 0;
            totalSize = 0;
            url = "";
        }

    }
}
