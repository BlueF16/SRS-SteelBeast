﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Caliburn.Micro;
using Ciribob.SRS.Common;
using Ciribob.SRS.Common.Network;
using Ciribob.SRS.Server.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Server.UI.ClientAdmin;
using Ciribob.DCS.SimpleRadio.Standalone.Server.UI.MainWindow;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Sentry;
using LogManager = NLog.LogManager;

namespace Ciribob.DCS.SimpleRadio.Standalone.Server
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _simpleContainer = new SimpleContainer();
        private bool loggingReady = false;

        public Bootstrapper()
        {
            SentrySdk.Init("https://ab4791abe55a4ae384a5a802c3060bc4@o414743.ingest.sentry.io/6011651");

            Initialize();
            SetupLogging();

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

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
                FileName = "serverlog.txt",
                ArchiveFileName = "serverlog.old.txt",
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
        }


        protected override void Configure()
        {
            _simpleContainer.Singleton<IWindowManager, WindowManager>();
            _simpleContainer.Singleton<IEventAggregator, EventAggregator>();
            _simpleContainer.Singleton<ServerState>();

            _simpleContainer.Singleton<MainViewModel>();
            _simpleContainer.Singleton<ClientAdminViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _simpleContainer.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _simpleContainer.GetAllInstances(service);
        }


        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            IDictionary<string, object> settings = new Dictionary<string, object>
            {
                {"Icon", new BitmapImage(new Uri("pack://application:,,,/SRS-Server;component/server-10.ico"))},
                {"ResizeMode", ResizeMode.CanMinimize}
            };
            //create an instance of serverState to actually start the server
            _simpleContainer.GetInstance(typeof(ServerState), null);

            DisplayRootViewFor<MainViewModel>(settings);
        }

        protected override void BuildUp(object instance)
        {
            _simpleContainer.BuildUp(instance);
        }


        protected override void OnExit(object sender, EventArgs e)
        {
            var serverState = (ServerState) _simpleContainer.GetInstance(typeof(ServerState), null);
            serverState.StopServer();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (loggingReady)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error(e.Exception, "Received unhandled exception, exiting");
            }

            base.OnUnhandledException(sender, e);
        }
    }
}