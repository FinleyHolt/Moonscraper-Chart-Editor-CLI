using System;
using UnityEngine;
using MoonscraperChartEditor.Song;
using MoonscraperChartEditor.Song.IO;

namespace MoonscraperChartEditor.CLI
{
    public class ChartConverterCLI
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ChartConverterCLI <input_directory> <output_directory>");
                Console.WriteLine("Converts Guitar Hero/Rock Band MIDI files to Clone Hero chart files");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  --resolution=N     Target resolution (default: 192)");
                Console.WriteLine("  --no-lyrics-fix    Don't substitute Clone Hero lyric characters");
                Console.WriteLine("  --no-copy-down     Don't copy down from harder difficulties");
                return 1;
            }

            string inputDir = args[0];
            string outputDir = args[1];

            int resolution = 192;
            bool fixLyrics = true;
            bool copyDown = true;

            // Parse options
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("--resolution="))
                {
                    if (!int.TryParse(args[i].Split('=')[1], out resolution))
                    {
                        Console.WriteLine("Invalid resolution value");
                        return 1;
                    }
                }
                else if (args[i] == "--no-lyrics-fix")
                {
                    fixLyrics = false;
                }
                else if (args[i] == "--no-copy-down")
                {
                    copyDown = false;
                }
                else
                {
                    Console.WriteLine($"Unknown option: {args[i]}");
                    return 1;
                }
            }

            var exportOptions = new ExportOptions
            {
                targetResolution = resolution,
                format = ExportOptions.Format.Chart,
                forced = true,
                copyDownEmptyDifficulty = copyDown,
                substituteCHLyricChars = fixLyrics,
                isGeneralSave = true
            };

            try
            {
                BatchCLI.ProcessDirectory(inputDir, outputDir, exportOptions);
                Console.WriteLine("Conversion complete!");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during conversion: {e.Message}");
                return 1;
            }
        }
    }
}
