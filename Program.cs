using mc_res_downloader;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

#region Funciones
void WL(string message)
{
    W(message + Environment.NewLine);
}
string LAST_MESSAGE_PRINTED = "";
void W(string message, bool replace = true, string replaceIfStartsWith = "", int? replaceIfMatchFirstNChars = null)
{
    bool r = replaceIfMatchFirstNChars != null ? LAST_MESSAGE_PRINTED.StartsWith(message.Substring(0, replaceIfMatchFirstNChars??0)) : false;
    if (replace || LAST_MESSAGE_PRINTED.StartsWith(replaceIfStartsWith) || r )
    {
        string space = "";
        for (int i = 0; i < Console.WindowWidth; i++)
            space += " ";

        Console.CursorLeft = 0;
        Console.Write(space);
        Console.CursorLeft = 0;
    }
    Console.Write(message);
    LAST_MESSAGE_PRINTED = message;
}
string R(string message = "")
{
    Console.Write(message);
    LAST_MESSAGE_PRINTED = message;
    return Console.ReadLine()??"";
}
void C() => Console.Clear();
#endregion

WebClient webClient = new();
Minedex? minedex;
MineIndexProf? mineIndexProf = null;
string jsonContent;

string mcDir = R("Ruta de MC (def: " + Minedex.MC_DIR + "): ");
Minedex.MC_DIR = string.IsNullOrEmpty(mcDir) ? Minedex.MC_DIR : mcDir;

string ver = R("Versión: ");
string assetIndexFile = Minedex.ensureDir("/tmp/", "idx_" + ver + ".json");

C();
try
{
    W("Descargando lista de assets para la versión " + ver + "... ");
    webClient.DownloadFile(
        string.Format(Minedex.indexUrlPlaceholder, ver),
        assetIndexFile
    );

    jsonContent = File.ReadAllText(assetIndexFile);
    mineIndexProf = JsonSerializer
        .Deserialize<MineIndexProf>(
            jsonContent, 
            new JsonSerializerOptions { 
                PropertyNameCaseInsensitive = true, 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    

    WL("Ok");

} catch(Exception x) { 
    WL("No se pudo obtener la información. Error: " + x.Message);
    return;
}

if (mineIndexProf == null) return;

try
{
    string indexFile = Minedex.ensureDir("/assets/indexes/", mineIndexProf.assetIndex.id + ".json");
    W("Descargando indice de recursos... ");
    webClient.DownloadFile(
        mineIndexProf.assetIndex.url,
        indexFile
    );

    jsonContent = File.ReadAllText(indexFile);
    minedex = JsonSerializer.Deserialize<Minedex>(jsonContent);
    WL("Ok");
}
catch (Exception x)
{
    WL("No se pudo leer el archivo de recursos. Error: " + x.Message);
    return;
}

if (minedex != null)
{
    int total = minedex.objects.Count;
    int current = 0;
    foreach(var key in minedex.objects.Keys)
    {
        current++;
        W(string.Format("[{1}/{2}] Obteniendo el archivo {0}", key, current, total), replaceIfMatchFirstNChars: 8);
        var obj = minedex.objects[key];
        using (var md5 = MD5.Create())
        {
            //"\\assets\\objects\\"
            string f = Minedex.ensureDir("/assets/objects/" + obj.hash.Substring(0,2) + "/", obj.hash);
            if (File.Exists(f)) {
                using (FileStream stream = File.OpenRead(f))
                {
                    using (SHA1Managed sha = new SHA1Managed())
                    {
                        byte[] checksum = sha.ComputeHash(stream);
                        string hash = BitConverter.ToString(checksum)
                            .Replace("-", string.Empty)
                            .ToLower();
                        if (hash != obj.hash) goto download;
                        else
                        {
                            WL(string.Format("Ya existe el archivo {0}", key));
                            continue;
                        }
                    }
                }
            }

            download:
            try
            {
                webClient.DownloadFile(obj.url??"", f);
            }
            catch (Exception x)
            {
                WL(x.Message);
                return;
            }
        }
    }
}

