# SpotRemover
Tool to extract Playlists from Spotify

## Usage
```
# start cmd.exe
% cd <path_to_SpotRemover>
% .\SpotRemover.exe <spotif_playlist_id>
```

Generates a tab separated (tsv) flat file under C:\\Users\\<user>\\AppData\\Local\\Temp\\_SpotRemover_<playlist_id>_<timestmp>.tsv containing all the songs in your playlist including details

The playListID can be retrieved from the Web URL when browsing Spotify.com

For exmaple with URL: `https://open.spotify.com/playlist/4WYNuRzicbUV1M2PIdKh5L`, the playlist id is 4WYNuRzicbUV1M2PIdKh5L .

To be able to use this tool, your playlists must be public.

## Liked Songs
To [extract liked songs](https://open.spotify.com/playlist/4WYNuRzicbUV1M2PIdKh5L), you need to copy the liked songs over to a new Playlist. This can only be done using the Spotify Desktop App.

The desktop app is available for Windows, MacOS and Linux. For linux use `snap`.

### Creating New Playlist from Liked Songs
* Open the App and Log in
* Open your Liked Songs, and then press Ctrl + A (on Windows) or command + A (on Mac) within your Liked Songs to select all the tracks
* Press Ctrl + C (on Windows) or command + C (on Mac) to copy the selected tracks
* Create a new playlist, open it, and then press Ctrl + V (on Windows) or command + V (on Mac) to paste the copied songs there
  * Wait for the pasting to complete. 
  * This can be very slow for long lists, so be patient.
  * You can use process explorer to monitor the progress of the app.


## Release 1.0.1 zip Hashes
|type|data|
|---|---|
|MD5|3463f80a17dd48ba084a7f4dcd92661d|
|SHA-1|93610a3b81d300bb3aa9067a91db18476401a6b7|
|SHA-256|dc27a029b2d1825e371b463dc5c562ac268fa9d89d3b0f4743bbcad0d8c3d687|
|VirusTotal|https://www.virustotal.com/gui/file/dc27a029b2d1825e371b463dc5c562ac268fa9d89d3b0f4743bbcad0d8c3d687/details|


## Virus scanner notes
* The source code is available but if your virus scanner uses MaxSecure with VirusTotal, you may get a false positive for `Trojan.Malware.300983.susgen`. This is a very common false positive that I have no idea of how to get around besides rewriting it in another language.
