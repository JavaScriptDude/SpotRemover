// SpotRemover - Spootify Playlist Extractor
// Extracts Spotify Playlist to TSV file
// Usage: SpotRemover.exe <Spotify Playlist ID>


using System.Diagnostics;
using System.Text;
using SpotifyExplode;
using System.Management;

namespace SpotRemover;

public static class SpotRemover {

    static async Task Main(string[] args) {

        if (GetParentProcessName().ToLower() == "explorer"){
            Console.Title = "SpotRemover Playlist Extractor";
            pc("This tool is intended to be run from the Command Line");
            pc("\nPlease use cmd.exe to call this program.");
            pc("\neg:");   
            pc(" % cd <SpotRemover_path>");   
            pc(" % .\\SpotRemover.exe <spotify_playlist_id>");   
            Console.Write("\nPress any key to exit...");
            Console.ReadKey();
            exit("Exiting...", 0);
        }

        var spotify = new SpotifyClient();

        if (args.Length != 1) {
            exit("Usage: SpotRemover <spotify_playlist_id>" +
                 "\nGenerates tsv file with playlist under C:\\Users\\<user>\\AppData\\Local\\Temp\\_SpotRemover_<playlist_id>_<timestmp>.tsv" +
                 "\nProject Home: ", 0);
            return;
        }

        var _pl_id = args[0];

        var url = $"https://open.spotify.com/playlist/{_pl_id}";

        pc("getting plalist info...");

        SpotifyExplode.Playlists.Playlist _pl;

        try {
            _pl = await spotify.Playlists.GetAsync(url);
        } catch (Exception e){
            exit($"Error retrieving playlist: {e.Message}");
            return;
        }

        pc($". got playlist {_pl.Name}");

        List<SpotifyExplode.Tracks.Track> _tracks;

        try {
            _tracks = await spotify.Playlists.GetAllTracksAsync(url);
        } catch (Exception e){
            exit($"Error retrieving tracks: {e.Message}");
            return;
        }

        var _timestamp = getMachineDTMS();
        var out_dir = System.IO.Path.GetTempPath();
        var out_file = $"_SpotRemover_{_pl_id}_{_timestamp}.tsv";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Artist\tName\tDur_s\tAlbum\tDate\tAlbum_Id\tTotalTracks\tDiscNumber\tTrackNumber\tArtistId\tOtherArtists");

        pc($". Listing Tacks for {_pl.Name}:");
        Console.Write(". building tsv ");
        _tracks.ForEach(track => {
            //pc($"title: {track.Title}, dur: {track.DurationMs}, album: {track.Album}");
            Console.Write(".");
            sb.Append(track.Artists[0].Name); sb.Append("\t");
            sb.Append(track.Title); sb.Append("\t");
            sb.Append(Math.Round(track.DurationMs/1000d)); sb.Append("\t");
            sb.Append(track.Album.Name); sb.Append("\t");
            sb.Append(track.Album.ReleaseDateStr); sb.Append("\t");
            sb.Append(track.Album.Id); sb.Append("\t");
            sb.Append(track.Album.TotalTracks); sb.Append("\t");
            sb.Append(track.DiscNumber); sb.Append("\t");
            sb.Append(track.TrackNumber); sb.Append("\t");
            sb.Append(track.Artists[0].Id); sb.Append("\t");
            if (track.Artists.Count == 1){
                sb.Append("-");
            } else {
                sb.Append(string.Join(", ", track.Artists.Skip(1).Select(a => a.Name)));
            }
            sb.AppendLine();

        });

        sb.Append("meta:\t");
        sb.Append("{ alias:\ttSpotRemover Extract"); 
        sb.Append("\t, playlist_id:\t"); sb.Append(_pl_id);
        sb.Append("\t, playlist_name:\t"); sb.Append(_pl.Name);
        sb.Append("\t, downloaded:\t"); sb.Append(_timestamp);
        sb.Append("}");


        Console.WriteLine();
        
        pc($". songs written: {_tracks.Count}");

        var _out_path = System.IO.Path.Combine(out_dir, out_file);
        System.IO.File.WriteAllText(_out_path, sb.ToString());

        pc($". Playlist {_pl.Name} written to {_out_path}");


        exit("~ done ~", 0);

    }

    public static Process GetParent(this Process process)
    {
      try
      {
#pragma warning disable CA1416 // Validate platform compatibility
            using (var query = new ManagementObjectSearcher(
          "SELECT * " +
          "FROM Win32_Process " +
          "WHERE ProcessId=" + process.Id))
        {
#pragma warning disable CS8603 // Possible null reference return.
                return query
            .Get()
            .OfType<ManagementObject>()
            .Select(p => Process.GetProcessById((int)(uint)p["ParentProcessId"]))
            .FirstOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
            }
#pragma warning restore CA1416 // Validate platform compatibility
        }
      catch
      {
        return null;
      }
    }

    public static string GetParentProcessName(){
        var _parent = Process.GetCurrentProcess().GetParent();
        if (_parent == null) return "_unknown_";
        return _parent.ProcessName;
    }

    static readonly Action<string> pc = (string s) => Console.WriteLine(s);
    static string getMachineDTMS(DateTime dt = default(DateTime)){ 
        return (dt == DateTime.MinValue ? DateTime.Now : dt).ToString("yyMMdd-HHmmss.fff"); 
    } 



    static void exit(string msg, int code = 1) {
        pc(msg);
        Debugger.Break();
        Environment.Exit(code);
    }


}
