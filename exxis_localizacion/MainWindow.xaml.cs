using exxis_localizacion.dataccess;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public PaginaParams PagParams { get; private set; }
        public PaginaEjecucion PagEjecucion { get; private set; }
        public PaginaEstructura PagEstructura { get; private set; }
        public PaginaCargando PagCargando { get; private set; }
        public PaginaPaquetes PagPaquetes { get; private set; }

        public SB1DataAccess SB1DataAccess { get; private set; } = new SB1DataAccess();
        public MainWindow()
        {
            DataContext = this;
            PagParams = new PaginaParams(this);
            PagEjecucion = new PaginaEjecucion(this);
            PagEstructura = new PaginaEstructura(this);
            PagCargando = new PaginaCargando(this);
            PagPaquetes = new PaginaPaquetes(this);

            InitializeComponent();
            SetUpLogger();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            frmPrincipal.Content = PagEstructura;
        }
        private static void SetUpLogger()
        {
            NameValueCollection _settings = System.Configuration.ConfigurationManager.AppSettings;

            Hierarchy hierarchy = (Hierarchy)log4net.LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            roller.File = @"Logs\EventLog.txt";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "5MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            ConsoleAppender console = new ConsoleAppender();
            console.Layout = patternLayout;
            console.ActivateOptions();
            hierarchy.Root.AddAppender(console);

            hierarchy.Root.Level = hierarchy.LevelMap[_settings.Get("loglevel")] ?? log4net.Core.Level.Error;
            hierarchy.Configured = true;
        }
    }
}
