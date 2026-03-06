using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace exxis_localizacion
{
    /// <summary>
    /// Lógica de interacción para PaginaEstructura.xaml
    /// </summary>
    public partial class PaginaEstructura : Page
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        MainWindow _frmPrincipal;
        public PaginaEstructura(MainWindow pFrmPrincipal)
        {
            DataContext = this;
            _frmPrincipal = pFrmPrincipal;
            InitializeComponent();
        }

        private void btnContinuar_Click(object sender, RoutedEventArgs e)
        {
            try 
            { 
                if ((bool)rbtnUIConn.IsChecked)
                {
                    _frmPrincipal.PagCargando.Mostrar();
                    Thread T = new Thread(new ThreadStart(() =>
                    {
                        bool sb1conectado = false;
                        if (_frmPrincipal.SB1DataAccess.ConectarUI())
                        {
                            if (_frmPrincipal.SB1DataAccess.ConectarDI())
                            {
                                sb1conectado = true;
                                logger.Debug($"Conectado a {_frmPrincipal.SB1DataAccess.SB1Company.CompanyName}");
                            }
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            if (sb1conectado)
                            {
                                _frmPrincipal.PagPaquetes.Mostrar();
                            }
                            else
                            {
                                _frmPrincipal.Content = this;
                                MessageBox.Show("Error en la conexión DI. Revisar log");
                            }
                        });
                    }));
                    T.Start();
                }
                else 
                {
                    MessageBox.Show("Opción no implementada");
                }            
            }
            catch(Exception ex) 
            {
                _frmPrincipal.Content = this;
                logger.Error("btnContinuar_Click",ex);
                MessageBox.Show(ex.Message);
            }
        }
    }
}
