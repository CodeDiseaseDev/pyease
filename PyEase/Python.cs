﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PyEase
{
    class Python
    {
        string pythonDir = $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python";

        public bool HasEnoughSpace(char driveLetter, long bytesSize)
        {
            DriveInfo drive = DriveInfo.GetDrives().Where(e => e.Name.Split(' ').Last()[0] == driveLetter).ToArray()[0]; // messy code lol

            return drive.AvailableFreeSpace > bytesSize;
        }

        public void RunScript(string code, ConsoleControl.ConsoleControl control, Action done)
        {
            string temp = Path.GetTempPath();
            string file = Path.GetRandomFileName() + ".py";
            string path = Path.Combine(temp, file);
            byte[] data = Encoding.UTF8.GetBytes(code);
            if (!HasEnoughSpace(temp[0]/*the first character of the temp directory will be the drive letter*/, data.Length))
            {
                MessageBox.Show("Not enough space on drive!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            File.WriteAllBytes(path, data);
            Task.Run(() =>
            {

                // For some reason python.exe isn't everywhere
                control.StartProcess("py.exe", path);
                while (control.ProcessInterface.IsProcessRunning);
                done();
            });
        }

        public bool IsPythonInstalled()
        {
            return Directory.Exists(pythonDir);
        }

        public void DeletePackage(string name)
        {
            foreach (string n in GetModulesNoFilter().Where(e=>e.Contains(name)))
                Directory.Delete(GetPackagePath(n), true);
        }

        public void InstallPackage(string name, ConsoleControl.ConsoleControl control, Action done)
        {
            if (control.IsProcessRunning) control.StopProcess();
            control.StartProcess("py.exe", $"-m pip install {name}");
            Task.Run(() =>
            {
                while (control.ProcessInterface.IsProcessRunning);
                done();
            });
        }

        public string GetPackagePath(string name)
        {
            // Not the best way to find a directory since many people don't have python 3.9 installed
            return Path.Combine(pythonDir, @"Python39\Lib\site-packages", name);
        }

        public string[] GetModules()
        {
            return GetModulesNoFilter().Select(c => { return c.Split('\\').Last(); }).Where(e => !e.Contains("-")&& !e.Contains("_")).ToArray();
        }

        public string[] GetModulesNoFilter()
        {
            // Not the best way to find a directory since many people don't have python 3.9 installed
            string mdir = Path.Combine(pythonDir, @"Python39\Lib\site-packages");
            return Directory.GetDirectories(mdir).ToArray();
        }
    }
}
