﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectInitializer
{
    /// <summary>
    /// This progam is a tool for the FFWD template that helps you to rename the existing template to fit your project.
    /// </summary>
    class Program
    {
        public string projectName;
        public string templateName = "FFWD.Template";
        public string startingDir = "XNA";
        public string[] ignoreDirs = new string[] { "bin", "obj" };
        public string[] ignoreFileTypes = new string[] { ".ncrunchsolution", ".user", ".cachefile", ".docstates" };
        public string[] fixFileContentsOfFileTypes = new string[] { ".csproj", ".sln" };

        static void Main(string[] args)
        {
            Program p = new Program();
            if (args.Length != 1)
            {
                Console.WriteLine("What is the name of your new project:");
                p.projectName = Console.ReadLine();
            }
            else
            {
                p.projectName = args[0];
            }
#if DEBUG
            p.startingDir = @"..\..\..\..\..\..\XNA";
#endif
            p.Execute();
        }

        private void Execute()
        {
            if (!Directory.Exists(startingDir))
            {
                Console.WriteLine("This program needs to be run in the root of the FFWD template project.");
                return;
            }

            CopyContents(startingDir, startingDir);
        }

        private void RenameSubdirs(string path)
        {
            foreach (var item in Directory.EnumerateDirectories(path))
            {
                if (ignoreDirs.Contains(Path.GetFileName(item)))
                {
                    continue;
                }
                if (item.Contains(templateName))
                {
                    string newDir = item.Replace(templateName, projectName);
                    Directory.CreateDirectory(newDir);
                    CopyContents(item, newDir);
                }
            }
        }

        private void CopyContents(string dir, string newDir)
        {
            foreach (var item in Directory.EnumerateFiles(dir))
            {
                string newFileName = Path.GetFileName(item);
                if (newFileName.Contains(templateName))
                {
                    newFileName = newFileName.Replace(templateName, projectName);
                }
                if (!ignoreFileTypes.Contains(Path.GetExtension(newFileName)))
                {
                    File.Copy(item, Path.Combine(newDir, newFileName));
                    if (fixFileContentsOfFileTypes.Contains(Path.GetExtension(newFileName)))
                    {
                        FixFileContents(Path.Combine(newDir, newFileName));                       
                    }
                }
            }
            RenameSubdirs(dir);
        }

        private void FixFileContents(string newFileName)
        {
            string text = File.ReadAllText(newFileName);
            text = text.Replace(templateName, projectName);
            File.WriteAllText(newFileName, text);
        }
    }
}
