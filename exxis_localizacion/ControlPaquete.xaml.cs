using exxis_localizacion.datasource;
using exxis_localizacion.entidades;
using exxis_localizacion.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para ControlPaquete.xaml
    /// </summary>
    public partial class ControlPaquete : UserControl, INotifyPropertyChanged
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public static readonly DependencyProperty PaqueteProperty = DependencyProperty.Register("Paquete", typeof(SB1Package), typeof(ControlPaquete));

        public event PropertyChangedEventHandler PropertyChanged;

        public SB1Package Paquete
        {
            get { return GetValue(PaqueteProperty) as SB1Package; }
            set
            {
                SetValue(PaqueteProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Paquete"));
            }
        }
 
        public ControlPaquete()
        {
            InitializeComponent();
        }
        public void DescargarTablas(object sender, RoutedEventArgs e) => Paquete.DescargarTablas();
        public void DescargarCampos(object sender, RoutedEventArgs e) => Paquete.DescargarCampos();
        public void DescargarObjetos(object sender, RoutedEventArgs e) => Paquete.DescargarObjetos();
        public void DescargarDatosTablas(object sender, RoutedEventArgs e) => Paquete.DescargarDatosTablas();
        public void DescargarDatosObjetos(object sender, RoutedEventArgs e) => Paquete.DescargarDatosObjetos();
        public void DescargarGestorConsultas(object sender, RoutedEventArgs e) => Paquete.DescargarGestorConsultas();
        public void DescargarScripts(object sender, RoutedEventArgs e) => Paquete.DescargarScripts();
        public void DescargarGEP(object sender, RoutedEventArgs e) => Paquete.DescargarGEP();
        public void DescargarCrystals(object sender, RoutedEventArgs e) => Paquete.DescargarCrystals();
        public void DescargarVistas(object sender, RoutedEventArgs e) => Paquete.DescargarVistas();
        public void AbrirUbicacion(object sender, RoutedEventArgs e) => Paquete.AbrirUbicacionModeloVistas();
        private void tab_MediaEnded(object sender, RoutedEventArgs e)
        {
            //Por algún motivo no funciona asignar TimeSpan.Zero
            MediaElement tab = (MediaElement)sender;
            if (!tab.Source.ToString().EndsWith(".gif")) return;
            tab.Position = new TimeSpan(0, 0, 1);
            tab.Play();
        }
        private void download_MediaEnded(object sender, RoutedEventArgs e)
        {
            //Por algún motivo no funciona asignar TimeSpan.Zero
            MediaElement tab = (MediaElement)sender;
            if (!tab.Source.ToString().EndsWith(".gif")) return;
            tab.Position = new TimeSpan(0, 0, 1);
            tab.Play();
        }
    }
}
