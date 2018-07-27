using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioConverter
{
    class Program
    {
        private static string _baseDirectoryPath = ".";
        private static string _assetsDirectory = "assets";
        private static string _outputDirectory = "output";
        private static string _binariesDirectory = "binaries";
        private static string _ffmpegBinaries = $@"{_baseDirectoryPath}\{_binariesDirectory}\ffmpeg.exe";
        private static string _assetsDirectoryPath = $@"{_baseDirectoryPath}\{_assetsDirectory}";
        private static string _outputDirectoryPath = $@"{_baseDirectoryPath}\{_outputDirectory}";
        private static string _inputFileExtension = "m4a";//wav or mp3 or any compatible audio type
        private static string _outputFileExtension = "mp3";// wav or mp3 or any compatible audio type (see the AudioConversion ffmpegProcess.StartInfo.Arguments part)

        static void Main(string[] args)
        {
            // Beginning of the output directory cleaning 

            foreach (string currentOutputFile in Directory.GetFiles(_outputDirectoryPath, $"*.{_inputFileExtension}"))
            {
                File.Delete(currentOutputFile);
            }

            // End of the output directory cleaning 

            // Beginning of the conversion process

            string[] assetsFiles = Directory.GetFiles(_assetsDirectoryPath, $"*.{_inputFileExtension}");

            foreach (string curentSrcFile in assetsFiles)
            {
                AudioConversion(curentSrcFile);
            }

            // End of the conversion process

            Console.WriteLine("END");

            //Console.ReadLine();
        }

        private static void AudioConversion(string srcFilePath)
        {
            // Recovering the source filename to correctly name the destination file (with the new extension)

            FileInfo fi = new FileInfo(srcFilePath);
            string ext = fi.Extension;
            string destFileName = fi.Name.Replace(ext, string.Empty) + $".{_outputFileExtension}";

            string outputFilePath = $@"{_baseDirectoryPath}\{_outputDirectory}\{destFileName}";

            // Configuring and executing the ffmpeg Process

            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo.FileName = _ffmpegBinaries;
                ffmpegProcess.StartInfo.Arguments = $"-i \"{srcFilePath}\" -codec:a libmp3lame -qscale:a 0 \"{outputFilePath}\"";
                //ffmpegProcess.StartInfo.Arguments = string.Format("-i \"{0}\" -codec:a aac \"{1}\"", srcFilePath, outputFilePath);//To convert to m4a file
                //ffmpegProcess.StartInfo.Arguments = string.Format("-i \"{0}\" -acodec pcm_u8 -ar 22050 \"{1}\"", srcFilePath, outputFilePath);//To convert to wav file

                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.EnableRaisingEvents = true;

                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.CreateNoWindow = true; // false for release

                ffmpegProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);

                ffmpegProcess.Start();
                ffmpegProcess.BeginOutputReadLine();
                ffmpegProcess.BeginErrorReadLine();

                ffmpegProcess.WaitForExit(60000);

                if (ffmpegProcess.HasExited)
                {
                    ffmpegProcess.CancelErrorRead();
                    ffmpegProcess.CancelOutputRead();
                    ffmpegProcess.Close();
                }
            }
        }

        private static void ErrorDataHandler(object sendingProcess, DataReceivedEventArgs errLine)
        {
            if (!string.IsNullOrEmpty(errLine.Data))
            {
                Console.WriteLine(errLine.Data);
            }
        }
    }
}
