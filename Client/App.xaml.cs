﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Ciribob.FS3D.SimpleRadio.Standalone.Client.Settings;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Sentry;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SRSClient
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool loggingReady = false;
        private static Logger Logger = LogManager.GetCurrentClassLogger();
  
        public App()
        {
            SentrySdk.Init("https://278831323bbb4efb94e17bc21b5f881d@o414743.ingest.sentry.io/6011780");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            var location = AppDomain.CurrentDomain.BaseDirectory;
            //var location = Assembly.GetExecutingAssembly().Location;

            //check for opus.dll
            if (!File.Exists(location + "\\opus.dll"))
            {
                MessageBox.Show(
                    $"You are missing the opus.dll - Reinstall using the Installer and don't move the client from the installation directory!",
                    "Installation Error!", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Environment.Exit(1);
            }

            if (!File.Exists(location + "\\speexdsp.dll"))
            {
                MessageBox.Show(
                    $"You are missing the speexdsp.dll - Reinstall using the Installer and don't move the client from the installation directory!",
                    "Installation Error!", MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Environment.Exit(1);
            }

            SetupLogging();

            ListArgs();


            if (IsClientRunning())
            {
                //check environment flag

                var args = Environment.GetCommandLineArgs();
                var allowMultiple = false;

                foreach (var arg in args)
                    if (arg.Contains("-allowMultiple"))
                        //restart flag to promote to admin
                        allowMultiple = true;

                if (GlobalSettingsStore.Instance.GetClientSettingBool(GlobalSettingsKeys.AllowMultipleInstances) ||
                    allowMultiple)
                {
                    Logger.Warn(
                        "Another SRS instance is already running, allowing multiple instances due to config setting");
                }
                else
                {
                    Logger.Warn("Another SRS instance is already running, preventing second instance startup");

                    var result = MessageBox.Show(
                        "Another instance of the SimpleRadio client is already running!\n\nThis one will now quit. Check your system tray for the SRS Icon",
                        "Multiple SimpleRadio clients started!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);


                    Environment.Exit(0);
                    return;
                }
            }
        }

        private void ListArgs()
        {
            Logger.Info("Arguments:");
            var args = Environment.GetCommandLineArgs();
            foreach (var s in args) Logger.Info(s);
        }


        private bool IsClientRunning()
        {
            var currentProcess = Process.GetCurrentProcess();
            var currentProcessName = currentProcess.ProcessName.ToLower().Trim();

            foreach (var clsProcess in Process.GetProcesses())
                if (clsProcess.Id != currentProcess.Id &&
                    clsProcess.ProcessName.ToLower().Trim() == currentProcessName)
                    return true;

            return false;
        }

        /* 
         * Changes to the logging configuration in this method must be replicated in
         * this VS project's NLog.config file
         */
        private void SetupLogging()
        {
            // If there is a configuration file then this will already be set
            if (LogManager.Configuration != null)
            {
                loggingReady = true;
                return;
            }

            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget
            {
                FileName = "clientlog.txt",
                ArchiveFileName = "clientlog.old.txt",
                MaxArchiveFiles = 1,
                ArchiveAboveSize = 104857600,
                Layout =
                    @"${longdate} | ${logger} | ${message} ${exception:format=toString,Data:maxInnerExceptionLevel=1}"
            };

            var wrapper = new AsyncTargetWrapper(fileTarget, 5000, AsyncTargetWrapperOverflowAction.Discard);
            config.AddTarget("asyncFileTarget", wrapper);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, wrapper));

            LogManager.Configuration = config;
            loggingReady = true;

            Logger = LogManager.GetCurrentClassLogger();
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (loggingReady)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error((Exception)e.ExceptionObject, "Received unhandled exception, {0}",
                    e.IsTerminating ? "exiting" : "continuing");
            }
        }
    }
}