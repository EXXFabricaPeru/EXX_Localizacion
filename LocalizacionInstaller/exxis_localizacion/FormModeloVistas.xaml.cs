using exxis_localizacion.util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para FormModeloVistas.xaml
    /// </summary>
    public partial class FormModeloVistas : Window
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);
        string _path = "";

        public FormModeloVistas(Window pparent)
        {
            Owner = pparent;
            InitializeComponent();
        }
        public void Mostrar(string path)
        {
            _path = path;
            this.ShowDialog();
        }
        private void btnVerUbicacion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = System.IO.Path.GetDirectoryName(_path);

                Process.Start(path);
            }
            catch (Exception ex)
            {
                logger.Error("btnVerUbicacion_Click", ex);
            }
        }

        private void btnContinuar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}
