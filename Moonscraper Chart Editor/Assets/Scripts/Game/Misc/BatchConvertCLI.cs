using System;
using System.IO;
using System.Collections.Generic;
using MoonscraperChartEditor.Song;
using MoonscraperChartEditor.Song.IO;
using UnityEngine;

namespace MoonscraperChartEditor
{
    public class BatchConvertCLI
    {
        public class Options
        {
            public string InputDirectory { get; set; }
            public string OutputDirectory { get; set; }
            public bool IncludeAudio { get; set; } = true;
            public bool ProcessSubdirectories { get; set; } = false;
            public ExportOptions.Format OutputFormat { get; set; } = ExportOptions.Format.Chart;
        }

        public static int Main(string[] args)
        {
            try
            {
                var options = ParseCommandLine(args);
                if (options == null)
                {
                    PrintUsage();
                    return 1;
                }

                ProcessDirectory(options);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        private static Options ParseCommandLine(string[] args)
        {
            if (args.Length < 2)
                return null;

            var options = new Options();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-i":
                    case "--input":
                        if (++i < args.Length)
                            options.InputDirectory = args[i];
                        break;

                    case "-o":
                    case "--output":
                        if (++i < args.Length)
                            options.OutputDirectory = args[i];
                        break;

                    case "-r":
                    case "--recursive":
                        options.ProcessSubdirectories = true;
                        break;

                    case "--no-audio":
                        options.IncludeAudio = false;
                        break;

                    case "--format":
                        if (++i < args.Length && Enum.TryParse(args[i], true, out ExportOptions.Format format))
                            options.OutputFormat = format;
                        break;
                }
            }

            return options.InputDirectory != null && options.OutputDirectory != null ? options : null;
        }

        private static void ProcessDirectory(Options options)
        {
            var searchOption = options.ProcessSubdirectories ? 
                SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var midiFile in Directory.GetFiles(options.InputDirectory, "*.mid", searchOption))
            {
                try
                {
                    ProcessMidiFile(midiFile, options);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing {midiFile}: {ex.Message}");
                }
            }
        }

        private static void ProcessMidiFile(string midiPath, Options options)
        {
            var sourceDir = Path.GetDirectoryName(midiPath);
            var songName = Path.GetFileNameWithoutExtension(sourceDir);
            var targetDir = Path.Combine(options.OutputDirectory, songName);

            // Create song object and load MIDI
            var song = new Song();
            
            // Load audio files if present
            if (options.IncludeAudio)
            {
                LoadAudioFiles(song, sourceDir);
            }

            // Load metadata from song.ini if present
            var iniPath = Path.Combine(sourceDir, "song.ini");
            if (File.Exists(iniPath))
            {
                LoadMetadata(song, iniPath);
            }

            // Ensure output directory exists
            Directory.CreateDirectory(targetDir);

            // Export chart
            var exportOptions = new ExportOptions
            {
                format = options.OutputFormat,
                targetResolution = song.resolution,
                copyDownEmptyDifficulty = true,
                forced = true
            };

            var chartPath = Path.Combine(targetDir, "notes.chart");
            var writer = new ChartWriter(chartPath);
            ChartWriter.ErrorReport errorReport;
            writer.Write(song, exportOptions, out errorReport);

            if (errorReport.HasErrors)
            {
                Console.Error.WriteLine($"Warnings/Errors exporting {midiPath}:");
                Console.Error.WriteLine(errorReport.GetFullReport());
            }

            // Copy audio files if needed
            if (options.IncludeAudio)
            {
                CopyAudioFiles(sourceDir, targetDir);
            }

            Console.WriteLine($"Processed: {midiPath} -> {chartPath}");
        }

        private static void LoadAudioFiles(Song song, string sourceDir)
        {
            var audioFiles = new Dictionary<string, Song.AudioInstrument>
            {
                { "song.ogg", Song.AudioInstrument.Song },
                { "guitar.ogg", Song.AudioInstrument.Guitar },
                { "bass.ogg", Song.AudioInstrument.Bass },
                { "rhythm.ogg", Song.AudioInstrument.Rhythm },
                { "drums_1.ogg", Song.AudioInstrument.Drum },
                { "drums_2.ogg", Song.AudioInstrument.Drums_2 },
                { "drums_3.ogg", Song.AudioInstrument.Drums_3 },
                { "drums_4.ogg", Song.AudioInstrument.Drums_4 },
                { "vocals.ogg", Song.AudioInstrument.Vocals },
                { "crowd.ogg", Song.AudioInstrument.Crowd }
            };

            foreach (var audioFile in audioFiles)
            {
                var path = Path.Combine(sourceDir, audioFile.Key);
                if (File.Exists(path))
                {
                    song.SetAudioLocation(audioFile.Value, path);
                }
            }
        }

        private static void LoadMetadata(Song song, string iniPath)
        {
            var ini = new INIParser();
            ini.Open(iniPath);

            song.metaData.name = ini.ReadValue("song", "name", string.Empty);
            song.metaData.artist = ini.ReadValue("song", "artist", string.Empty);
            song.metaData.charter = ini.ReadValue("song", "charter", string.Empty);
            song.metaData.album = ini.ReadValue("song", "album", string.Empty);
            song.metaData.year = ini.ReadValue("song", "year", string.Empty);
            song.offset = ini.ReadValue("song", "delay", 0f);
        }

        private static void CopyAudioFiles(string sourceDir, string targetDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir, "*.ogg"))
            {
                var destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine(@"Usage: BatchConvertCLI [options]
Options:
  -i, --input <dir>     Input directory containing MIDI files
  -o, --output <dir>    Output directory for converted charts
  -r, --recursive       Process subdirectories recursively
  --no-audio            Don't include audio files in output
  --format <format>     Output format (Chart or Msce)");
        }
    }
}
