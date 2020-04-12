using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSynthesis
{
    static class Logger
    {
        private const string ErrorFile = @"\ERROR.txt";
        private const string OutputFile = @"\Log.txt";
        private static string ErrorDirectory { get; set; }
        private static string OutputDirectory { get; set; }
        private const string FlatLine = "------------------------\n";

        static Logger()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            ErrorDirectory = Path.GetDirectoryName(path).Replace(@"file:\", "") + ErrorFile;
            OutputDirectory = Path.GetDirectoryName(path).Replace(@"file:\", "") + OutputFile;
            File.WriteAllText(OutputDirectory, "");
        }

        public static void Write(string output)
        {
            Console.WriteLine(output);
            output = DateTime.UtcNow + ": " + output;
            File.AppendAllText(OutputDirectory, output + Environment.NewLine);
        }

        public static void ErrorHandler(Exception e, string currentProfile)
        {
            var output = DateTime.UtcNow + Environment.NewLine;
            output += FlatLine;
            output += "Please forward this file along with any information about what happened when the error occured to me here:" + Environment.NewLine;
            output += "https://github.com/Saad888/AutoSynthesis/issues " + Environment.NewLine;
            output += FlatLine;
            output += e.Message + Environment.NewLine;
            output += e.StackTrace + Environment.NewLine;
            Console.WriteLine(output);
            output += FlatLine;
            output += currentProfile + Environment.NewLine;
            output += FlatLine;
            try
            {
                output += File.ReadAllText(ProfileManager.Directory);

            }
            catch (Exception f)
            {
                output += "Unable to read profile file " + Environment.NewLine;
                output += f.Message;
            }
            output += FlatLine;
            try
            {
                output += File.ReadAllText(OutputDirectory);
            }
            catch (Exception f)
            {
                output += "Unable to read profile file " + Environment.NewLine;
                output += f.Message;
            }
            File.WriteAllText(ErrorDirectory, output);
        }
    }
}
