﻿using ClangPowerTools;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Commands
{
  public static class FileCommand
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);
    
    public static string GetShortPath(string longPath)
    {
      StringBuilder shortPath = new StringBuilder(255);
      GetShortPathName(longPath, shortPath, 255);
      return shortPath.ToString();
    }

    public static void TidyFixDiff(FileModel filePath, bool makeDiff = true)
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      var clangTidyPath = settingsPathBuilder.GetCurrentExecutableLlvmPath();
      try
      {
        FileInfo file = new(filePath.FullFileName);
        File.Copy(file.FullName, filePath.CopyFullFileName, true);
        System.Diagnostics.Process process = new();
        process.StartInfo.FileName = clangTidyPath;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Arguments = $"-fix \"{file.FullName}\"";
        process.Start();
        process.WaitForExit();
        if (makeDiff)
          DiffFilesUsingDefaultTool(filePath.CopyFullFileName, file.FullName);;
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Tidy-Diff Failed - you do not have an llvm set", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public static void CopyFileInTempSolution(FileModel file)
    {
      FileInfo fileInfo = new(file.CopyFullFileName);
      Directory.CreateDirectory(fileInfo.Directory.FullName);
      File.Copy(file.FullFileName, file.CopyFullFileName, true);
    }

    public static void Copy(string source, string destination)
    {
      System.Diagnostics.Process process = new();
      process.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.Arguments = $"Copy-Item -LiteralPath " + source + " " + destination;
      process.Start();
      process.WaitForExit();
    }

    public static void CopyFilesInTempSolution(List<FileModel> files)
    {
      foreach(var file in files)
      {
        CopyFileInTempSolution(file);
      }
    }

    public static void DiffFilesUsingDefaultTool(string file1, string file2)
    {
      object args = $"\"{file1}\" \"{file2}\"";
      var dte = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      dte.Commands.Raise(TidyConstants.ToolsDiffFilesCmd, TidyConstants.ToolsDiffFilesId, ref args, ref args);
    }
  }
}
