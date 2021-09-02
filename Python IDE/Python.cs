﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Python_IDE
{
    class Python
    {
        string pythonDir = $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Python";

        public void RunScript(string[] code, ConsoleControl.ConsoleControl control, Action done)
        {
            if (control.IsProcessRunning) control.StopProcess();
            string temp = Path.GetTempPath();
            string file = Path.GetRandomFileName() + ".py";
            string path = Path.Combine(temp, file);
            File.WriteAllLines(path, code);
            control.StartProcess("python.exe", path);
            Task.Run(() =>
            {
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
            control.StartProcess("python.exe", $"-m pip install {name}");
            Task.Run(() =>
            {
                while (control.ProcessInterface.IsProcessRunning);
                done();
            });
        }

        public string GetPackagePath(string name)
        {
            return Path.Combine(pythonDir, @"Python39\Lib\site-packages", name);
        }

        public string[] GetModules()
        {
            return GetModulesNoFilter().Select(c => { return c.Split('\\').Last(); }).Where(e => !e.Contains("-")&& !e.Contains("_")).ToArray();
        }

        public string[] GetModulesNoFilter()
        {
            string mdir = Path.Combine(pythonDir, @"Python39\Lib\site-packages");
            return Directory.GetDirectories(mdir).ToArray();
        }
    }
}
