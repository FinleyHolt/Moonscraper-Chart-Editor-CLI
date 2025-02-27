using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;
using System.Linq;

namespace MoonscraperChartEditor.Song.IO
{
    public class BatchCLI
    {
        public class SongPackage
        {
            public string MidiPath { get; set; }
            public string IniPath { get; set; }
            public Dictionary<Song.AudioInstrument, string> AudioPaths { get; set; }
            public string AlbumArtPath { get; set; }
            public string DirectoryName { get; set; }

            public SongPackage()
            {
                AudioPaths = new Dictionary<Song.AudioInstrument, string>();
            }
        }

        static readonly Dictionary<string, Song.AudioInstrument> AudioFileMapping = new Dictionary<string, Song.AudioInstrument>
        {
            {"song.ogg", Song.AudioInstrument.Song},
            {"guitar.ogg", Song.AudioInstrument.Guitar},
            {"bass.ogg", Song.AudioInstrument.Bass},
            {"rhythm.ogg", Song.AudioInstrument.Rhythm},
            {"drums_1.ogg", Song.AudioInstrument.Drum},
            {"drums_2.ogg", Song.AudioInstrument.Drums_2},
            {"drums_3.ogg", Song.AudioInstrument.Drums_3},
            {"drums_4.ogg", Song.AudioInstrument.Drums_4},
            {"vocals.ogg", Song.AudioInstrument.Vocals},
            {"crowd.ogg", Song.AudioInstrument.Crowd}
        };

        public static void ProcessDirectory(string inputPath, string outputPath, ExportOptions exportOptions)
        {
            if (!Directory.Exists(inputPath))
            {
                Debug.LogError($"Input directory {inputPath} does not exist");
                return;
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Find all directories containing .mid files
            var songPackages = FindSongPackages(inputPath);

            foreach (var package in songPackages)
            {
                try
                {
                    ProcessSongPackage(package, outputPath, exportOptions);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing {package.DirectoryName}: {e.Message}");
                }
            }
        }

        static List<SongPackage> FindSongPackages(string rootPath)
        {
            var packages = new List<SongPackage>();
            
            foreach (var dir in Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories))
            {
                var midiFiles = Directory.GetFiles(dir, "*.mid");
                if (midiFiles.Length > 0)
                {
                    var package = new SongPackage
                    {
                        MidiPath = midiFiles[0],
                        DirectoryName = Path.GetFileName(dir),
                        IniPath = Path.Combine(dir, "song.ini"),
                        AlbumArtPath = Path.Combine(dir, "album.png")
                    };

                    // Find audio files
                    foreach (var mapping in AudioFileMapping)
                    {
                        string audioPath = Path.Combine(dir, mapping.Key);
                        if (File.Exists(audioPath))
                        {
                            package.AudioPaths[mapping.Value] = audioPath;
                        }
                    }

                    packages.Add(package);
                }
            }

            return packages;
        }

        static void ProcessSongPackage(SongPackage package, string outputPath, ExportOptions exportOptions)
        {
            // Create output directory
            string outputDir = Path.Combine(outputPath, package.DirectoryName);
            Directory.CreateDirectory(outputDir);

            // Load and convert MIDI
            Song song = MidReader.ReadMid(package.MidiPath);
            if (song != null)
            {
                // Copy audio files
                foreach (var audioPair in package.AudioPaths)
                {
                    string destPath = Path.Combine(outputDir, Path.GetFileName(audioPair.Value));
                    File.Copy(audioPair.Value, destPath, true);
                    song.SetAudioLocation(audioPair.Key, destPath);
                }

                // Copy album art if it exists
                if (File.Exists(package.AlbumArtPath))
                {
                    string destArtPath = Path.Combine(outputDir, "album.png");
                    File.Copy(package.AlbumArtPath, destArtPath, true);
                }

                // Copy and process ini file
                if (File.Exists(package.IniPath))
                {
                    string destIniPath = Path.Combine(outputDir, "song.ini");
                    File.Copy(package.IniPath, destIniPath, true);
                    ProcessIniMetadata(package.IniPath, song);
                }

                // Write converted chart
                string outputChartPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(package.MidiPath) + ".chart");
                ChartWriter writer = new ChartWriter(outputChartPath);
                ErrorReport errorReport;
                writer.Write(song, exportOptions, out errorReport);

                if (errorReport.HasErrors)
                {
                    Debug.LogWarning($"Warnings/Errors while processing {package.DirectoryName}:\n{errorReport.GetFullReport()}");
                }
            }
        }

        static void ProcessIniMetadata(string iniPath, Song song)
        {
            try
            {
                var ini = new INIParser();
                ini.Open(iniPath);

                song.metaData.name = ini.ReadValue("song", "name", song.metaData.name);
                song.metaData.artist = ini.ReadValue("song", "artist", song.metaData.artist);
                song.metaData.charter = ini.ReadValue("song", "charter", song.metaData.charter);
                song.metaData.album = ini.ReadValue("song", "album", song.metaData.album);
                song.metaData.year = ini.ReadValue("song", "year", song.metaData.year);
                song.metaData.genre = ini.ReadValue("song", "genre", song.metaData.genre);

                float offset;
                if (float.TryParse(ini.ReadValue("song", "delay", "0"), out offset))
                {
                    song.offset = offset / 1000f; // Convert from ms to seconds
                }

                ini.Close();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error processing INI file {iniPath}: {e.Message}");
            }
        }
    }
}
