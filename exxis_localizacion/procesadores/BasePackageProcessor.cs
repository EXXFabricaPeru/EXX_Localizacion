using exxis_localizacion.dataccess;
using exxis_localizacion.entidades;
using System;
using System.Reflection;

namespace exxis_localizacion.procesadores
{
    public abstract class BasePackageProcessor
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public event EventHandler<PackageProcessorEventArgs> PackageProcessorEventHandler;

        protected int _progresoGeneral = 0;
        protected int _maxContadorGeneral = 0;
        protected string _mensajeGeneral;
        protected int _progresoParcial = 0;
        protected int _maxContadorParcial = 0;
        protected string _mensajeParcial;

        protected SAPbobsCOM.Company SB1Company;
        protected SB1DataAccess _SB1DataAccess;
        protected BasePackageProcessor(SB1DataAccess pSB1DataAccess)
        {
            _SB1DataAccess = pSB1DataAccess;
            _SB1DataAccess.ItemExecuted += _SB1DataAccess_ItemExecuted;
            SB1Company = pSB1DataAccess.SB1Company;
        }
        private void _SB1DataAccess_ItemExecuted(object sender, ItemExecutedEventArgs e)
        {
            PackageProcessorEventArgs args = new PackageProcessorEventArgs();
            args.ProgresoGeneral = (_progresoGeneral * 100 / _maxContadorGeneral);
            args.TextoGeneral = _mensajeGeneral;
            args.TextoParcial = _mensajeParcial;
            args.ProgresoParcial = (_progresoParcial * 100 / _maxContadorParcial);
            args.TextoResultadoParcial = e.Mensaje;
            args.Finalizado = false;
            PackageProcessorEventHandler?.Invoke(this, args);
        }
        public abstract void EjecutarProcesos(SB1Package pqt);
        protected void EnviarProgreso(int progresoParcial, int maxContadorParcial, String mensajeParcial, String resultadoParcial, bool finalizado, string vistas = null)
        {
            _progresoParcial = progresoParcial;
            _maxContadorParcial = maxContadorParcial;
            _mensajeParcial = mensajeParcial;
            PackageProcessorEventArgs args = new PackageProcessorEventArgs();
            args.ProgresoGeneral = (_progresoGeneral * 100 / _maxContadorGeneral);
            args.TextoGeneral = _mensajeGeneral;
            args.TextoParcial = _mensajeParcial;
            args.Vistas = vistas;
            args.ProgresoParcial = (_progresoParcial * 100 / _maxContadorParcial);
            args.TextoResultadoParcial = resultadoParcial;
            args.Finalizado = finalizado;
            PackageProcessorEventHandler?.Invoke(this, args);
        }
    }
}