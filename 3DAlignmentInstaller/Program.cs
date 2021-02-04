using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _3DAlignmentInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            var SourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var DestinationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2021";


            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (!SourcePath.Contains(".exe"))
                {
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);

                }
            }

            Console.WriteLine("Files Copied");
        }
    }
}
