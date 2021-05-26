using System;
using System.IO;
using System.IO.Compression;
using LoveShack.Properties;

//
// MIT License
// 
// Copyright (c) 2021 Pharap (@Pharap)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

namespace LoveShack
{
    class Program
    {
        static void Main(string[] args)
        {
            // Iterate through all arguments
            foreach (var path in args)
                try
                {
                    // Process the argument.
                    Process(path);
                }
                catch(Exception ex)
                {
                    // Log the error.
                    Console.Error.Write(ex);
                }
        }

        // Process the directory.
        static void Process(string directoryPath)
        {
            // Ensure the path is a directory.
            if (!Directory.Exists(directoryPath))
                throw new FormatException(string.Format("Argument was not a path to a valid directory: {0}", directoryPath));

            // Take the root of the directory.
            var targetRoot = Path.GetDirectoryName(directoryPath);

            // Take the name of the directory.
            var targetName = Path.GetFileName(directoryPath);

            // Combine the root, the name and a .love extension
            // to create the path of the zipped file.
            var lovePath = Path.Combine(targetRoot, targetName) + ".love";

            // Create the Zip (.love) file.
            ZipFile.CreateFromDirectory(directoryPath, lovePath);

            // If the path to the LOVE executable has been set...
            if (!string.IsNullOrEmpty(Settings.Default.LovePath))
            {
                // Check if the path is a valid file.
                if (!File.Exists(Settings.Default.LovePath))
                    throw new FormatException(string.Format("Path to Love executable could not be found: {0}", Settings.Default.LovePath));

                // Check if the path is an .exe file
                if (Path.GetExtension(Settings.Default.LovePath) != ".exe")
                    throw new FormatException(string.Format("Path to Love executable is not an executable: {0}", Settings.Default.LovePath));

                // Combine the root, the name and a .exe extension
                // to create the path of the executable file.
                var executablePath = Path.Combine(targetRoot, targetName) + ".exe";

                // Open the new file for writing.
                using (var executableFile = File.OpenWrite(executablePath))
                {
                    // Append the LOVE executable.
                    using (var loveExecutableFile = File.OpenRead(Settings.Default.LovePath))
                        loveExecutableFile.CopyTo(executableFile);

                    // Append the zipped .love file.
                    using (var loveFile = File.OpenRead(lovePath))
                        loveFile.CopyTo(executableFile);
                }

                // Combine the root, the name and a .zip extension
                // to create the path of the bundled Zip file.
                var zipPath = Path.Combine(targetRoot, targetName) + ".zip";

                using (var zipFile = File.OpenWrite(zipPath))
                using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    // Add the .exe file to the archive.
                    archive.CreateEntryFromFile(executablePath, Path.GetFileName(executablePath));

                    // Get the folder that contains the LOVE executable.
                    var loveRoot = Path.GetDirectoryName(Settings.Default.LovePath);

                    // Enumerate all .dll files in the directory of the LOVE executable
                    foreach (var file in Directory.EnumerateFiles(loveRoot, "*.dll"))
                    {
                        // Get the library file's name.
                        var fileName = Path.GetFileName(file);

                        // Create the target path.
                        var destinationPath = Path.Combine(targetRoot, fileName);

                        // Copy the library file.
                        File.Copy(file, destinationPath);

                        // Add the library file to the archive.
                        archive.CreateEntryFromFile(file, fileName);
                    }
                }
            }
        }
    }
}
