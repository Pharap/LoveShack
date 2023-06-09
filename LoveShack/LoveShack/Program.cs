using System;
using System.IO;
using System.IO.Compression;
using LoveShack.Properties;

//
// MIT License
// 
// Copyright (c) 2023 Pharap (@Pharap)
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

            // If a .love file already exists
            if (File.Exists(lovePath))
                // Delete it
                File.Delete(lovePath);

            // Create the Zip (.love) file.
            ZipFile.CreateFromDirectory(directoryPath, lovePath);

            // Combine the root, the name and a .exe extension
            // to create the path of the executable file.
            var executablePath = Path.Combine(targetRoot, targetName) + ".exe";

            // Try to find LOVE executable.
            var loveExecutablePath = FindLOVE();

            // Open the new file for writing.
            using (var executableFile = File.OpenWrite(executablePath))
            {
                // Append the LOVE executable.
                using (var loveExecutableFile = File.OpenRead(loveExecutablePath))
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
                var loveRoot = Path.GetDirectoryName(loveExecutablePath);

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

        // Try to find LOVE.
        static string FindLOVE()
        {
            // Read the executable path from settings.
            string result = Settings.Default.LovePath;

            // If the path has not been set...
            if (string.IsNullOrEmpty(result))
            {
                // Try to look for LOVE locally.
                if (!TryFindLOVELocally(out result))
                    // If LOVE can't be found locally, give up.
                    throw new FileNotFoundException("LOVE executable could not be found");
            }
            // If the path has been set...
            else
            {
                // Check if the path is an .exe file
                if (Path.GetExtension(result) != ".exe")
                    throw new FormatException(string.Format("Provided path to LOVE executable is not an executable: {0}", result));

                // Check if the path is a valid file.
                if (!File.Exists(result))
                    // If it hasn't, try to look for LOVE locally.
                    if (!TryFindLOVELocally(out result))
                        // If LOVE can't be found locally, give up.
                        throw new FileNotFoundException(string.Format("Path to LOVE executable could not be found: {0}", result));
            }

            return result;
        }

        // Try to find LOVE locally.
        static bool TryFindLOVELocally(out string result)
        {
            // Create a hypothetical path to a love.exe in LoveShack's directory.
            var possiblePath = Path.Combine(AppContext.BaseDirectory, "love.exe");

            // Check the path exists.
            if (File.Exists(possiblePath))
            {
                // If it does, that's the new path.
                result = possiblePath;
                return true;
            }
            else
            {
                // If it doesn't, fail.
                result = "";
                return false;
            }

        }
    }
}
