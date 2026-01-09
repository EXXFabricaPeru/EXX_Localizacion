using exxis_localizacion.entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exxis_localizacion.datasource
{
    public interface IResourceGrabber
    {
        SB1Package PaqueteActual { get; set; }
        List<TablaUsuarioWrp> GetUserTableList();
        List<CampoUsuarioWrp> GetUserFieldList();
        List<ObjetoUsuarioWrp> GetUserObjectList();
        List<CategoriaConsultaWrp> GetQueryCategoryList();
        List<ConsultaUsuarioWrp> GetUserQueryList(List<CategoriaConsultaWrp> listaCategoriaConsulta, string bdtype);
        List<BusquedaFormateadaWrp> GetFormattedSearchList(List<ConsultaUsuarioWrp> listaConsultaUsuario);
        IDictionary<string, object> GetUserTablesData();
        List<DatosObjetoWrp> GetUserObjectsData(SAPbobsCOM.Company sB1Company);
        List<FormatoElectronicoWrp> GetElectronicFormatList();
        List<FormatoCrystalWrp> GetCrystalFormatList(string bdtype);
        List<ScriptFile> GetScriptList(string bdtype);
        List<Script> GetScriptListNew(string bdtype);
        List<SB1Package> GetPackageList(string bdtype);
        string GetModeloVistas();
    }
}
