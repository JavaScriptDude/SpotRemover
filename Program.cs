// SpotRemover - Spootify Playlist Extractor
// Extracts Spotify Playlist to TSV file
// Usage: SpotRemover.exe <Spotify Playlist ID>


using System.Diagnostics;
using System.Text;
using SpotifyExplode;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using static System.Net.WebRequestMethods;


public static class SpotRemover {

    static async Task Main(string[] args) {
        var title = "SpotRemover Playlist Extractor";
        var ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        if (GetParentProcessName().ToLower() == "explorer"){
            Console.Title = $"{title} v{ver}";
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
                 "\nProject Home: https://github.com/JavaScriptDude/SpotRemover", 0);
            return;
        }

        var _pl_id = args[0];

        var url = $"https://open.spotify.com/playlist/{_pl_id}";

        pc($"{title} v{ver}");
        pc(". getting plalist info...");

        SpotifyExplode.Playlists.Playlist _pl;

        try {
            _pl = await spotify.Playlists.GetAsync(url);
        } catch (Exception e){
            exit($"Error retrieving playlist: {e.Message}");
            return;
        }

        pc($". found playlist {_pl.Name}. Getting tracks...");

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


        var rows = new List<Record>();



        pc($". Listing Tacks for {_pl.Name}:");
        Console.Write(". building tsv ");
        _tracks.ForEach(track => {
            var row = new Record();
            row.Artist = track.Artists[0].Name;
            row.Name = track.Title;
            row.Dur_s = (int)Math.Round(track.DurationMs / 1000d);
            row.Album = track.Album.Name;
            var date_str = track.Album.ReleaseDateStr;
            if (date_str.Length == 4) date_str += "-01-01";
            row.Date = date_str;
            row.Album_Id = track.Album.Id;
            row.TotalTracks = track.Album.TotalTracks;
            row.DiscNumber = track.DiscNumber;
            row.TrackNumber = track.TrackNumber;
            row.ArtistId = track.Artists[0].Id;
            if (track.Artists.Count == 1){ 
                row.OtherArtists = "-";
            } else {
                for (int i = 1; i < track.Artists.Count - 1; i++) {
                    if (i > 1) row.OtherArtists += ", ";
                    row.OtherArtists += track.Artists[i].Name;
                }
            }

            rows.Add(row);

        });


        Console.WriteLine();
        
        pc($". songs written: {_tracks.Count}");

        var _out_path = System.IO.Path.Combine(out_dir, out_file);

        using (var writer = new StreamWriter(_out_path)) {

            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
            config.Delimiter = "\t";

            using (var csv = new CsvWriter(writer, config)){
                csv.WriteHeader<Record>();
                csv.NextRecord();
                foreach (var row in rows){
                    csv.WriteRecord(row);
                    csv.NextRecord();
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("\n\nmeta:\t");
        sb.Append("{ alias:\ttSpotRemover Extract"); 
        sb.Append("\t, playlist_id:\t"); sb.Append(_pl_id);
        sb.Append("\t, playlist_name:\t"); sb.Append(_pl.Name);
        sb.Append("\t, downloaded:\t"); sb.Append(_timestamp);
        sb.Append("}");

        System.IO.File.AppendAllText(_out_path, sb.ToString());

        pc($". Playlist {_pl.Name} written to {_out_path}");


        exit("~ done ~", 0);

    }

    private struct Record
    {
        public string Artist { get; set; }
        public string Name { get; set; }
        public int Dur_s { get; set; }
        public string Album { get; set; }
        public string Date { get; set; }
        public string Album_Id { get; set; }
        public int TotalTracks { get; set; }
        public int DiscNumber { get; set; }
        public int TrackNumber { get; set; }
        public string ArtistId { get; set; }
        public string OtherArtists { get; set; }
    }




    public struct ParentProcessUtilities
    {
        // These members must match PROCESS_BASIC_INFORMATION
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess()
        {
            return GetParentProcess(Process.GetCurrentProcess().Handle);
        }

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(int id)
        {
            Process process = Process.GetProcessById(id);
            return GetParentProcess(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(IntPtr handle)
        {
            ParentProcessUtilities pbi = new ParentProcessUtilities();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
            if (status != 0)
                throw new Win32Exception(status);

            try
            {
                return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
    }


    public static string GetParentProcessName(){
        var _parent = ParentProcessUtilities.GetParentProcess();
        if (_parent == null) return "_unknown_";
        return _parent.ProcessName;
    }

    static readonly Action<string> pc = (string s) => Console.WriteLine(s);
    static string getMachineDTMS(DateTime dt = default(DateTime)){ 
        return (dt == DateTime.MinValue ? DateTime.Now : dt).ToString("yyMMdd-HHmmss.fff"); 
    } 



    static void exit(string msg, int code = 1) {
        pc(msg);
        if (Debugger.IsAttached) Debugger.Break();
        Environment.Exit(code);
    }


}
