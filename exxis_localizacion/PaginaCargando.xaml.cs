using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para PaginaCargando.xaml
    /// </summary>
    public partial class PaginaCargando : Page
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        MainWindow _frmPrincipal;

        public PaginaCargando(MainWindow pFrmPrincipal)
        {
            DataContext = this;
            _frmPrincipal = pFrmPrincipal;
            InitializeComponent();
        }

        private void cargador_MediaEnded(object sender, RoutedEventArgs e)
        {
            //Por algún motivo no funciona asignar TimeSpan.Zero
            cargador.Position = new TimeSpan(0, 0, 1);
            cargador.Play();
        }
        private void restartAnimaction()
        {
            cargador.Position = new TimeSpan(0, 0, 1);
            cargador.Play();
        }
        public void Mostrar() {
            restartAnimaction();
            _frmPrincipal.Content = this;
        }
    }
}
