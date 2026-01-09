using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExxisBibliotecaClases
{
    public class PermisosManager
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        private Company company;

        public PermisosManager(Company dicmp)
        {
            this.company = dicmp;
        }
        public void Check(String xmlpath)
        {
            try
            {
                int count = company.GetXMLelementCount(xmlpath);
                logger.Info($"Check: se han detectado {count} permisos");
                for (int i = 0; i < count; i++)
                {
                    CheckFromXML(xmlpath, i);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CheckFromXML(string xmlpath, int i)
        {
            SAPbobsCOM.UserPermissionTree uptcmp = null;
            SAPbobsCOM.UserPermissionTree uptapp = null;
            try
            {
                uptapp = (UserPermissionTree)company.GetBusinessObjectFromXML(xmlpath, i);
                uptcmp = (UserPermissionTree)company.GetBusinessObject(BoObjectTypes.oUserPermissionTree);

                if (uptcmp.GetByKey(uptapp.PermissionID))
                {
                    logger.Debug($"CheckFromXML: el permiso {uptapp.PermissionID} existe");
                    if (!CompareField(uptapp, uptcmp))
                    {
                        int ret = uptapp.Update();
                        if (ret != 0)
                        {
                            throw new Exception($"{uptapp.PermissionID}:({company.GetLastErrorCode()}) - ({company.GetLastErrorDescription()})");
                        }
                        logger.Info($"CheckFromXML: el permiso {uptapp.PermissionID} ha sido actualizado");
                    }
                    else
                    {
                        logger.Debug($"CheckFromXML: el permiso {uptapp.PermissionID} es correcto");
                    }
                }
                else
                {
                    logger.Debug($"CheckFromXML: el permiso {uptapp.PermissionID} no existe. Se intentará crear.");
                    int ret = uptapp.Add();
                    if (ret != 0)
                    {
                        throw new Exception($"{uptapp.PermissionID}:({company.GetLastErrorCode()}) - ({company.GetLastErrorDescription()})");
                    }
                    logger.Info($"CheckFromXML: el permiso {uptapp.PermissionID}  ha sido creado");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (uptapp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(uptapp);
                if (uptcmp != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(uptcmp);
                uptapp = null;
                uptcmp = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private bool CompareField(UserPermissionTree uptapp, UserPermissionTree uptcmp)
        {
            logger.Debug($"CompareField: validando permiso {uptapp.PermissionID} ");

            if (uptapp.Name != uptcmp.Name)
            {
                logger.Debug($"CompareField: {uptapp.Name} != {uptcmp.Name}");
                return false;
            }

            SAPbobsCOM.UserPermissionForms upfcmp = uptcmp.UserPermissionForms;
            SAPbobsCOM.UserPermissionForms upfapp = uptapp.UserPermissionForms;

            //Eliminar formularios de permiso de compañia. NO ES POSIBLE.
            //while (upfcmp.Count > 0) {
            //    upfcmp.SetCurrentLine(0);
            //    upfcmp.remove();
            //}

            for (int i = 0; i < upfapp.Count; i++)
            {
                upfapp.SetCurrentLine(i);
                bool existe = false;
                for (int j = 0; j < upfcmp.Count; j++)
                {
                    upfcmp.SetCurrentLine(j);
                    if (upfapp.FormType == upfcmp.FormType)
                    {
                        existe = true;
                        break;
                    }
                }
                if (!existe)
                {
                    logger.Debug($"CompareField: Formulario {upfapp.FormType} no existe");
                    return false;
                }
            }

            logger.Debug($"CompareField: son idénticos");

            return true;
        }
    }
}
