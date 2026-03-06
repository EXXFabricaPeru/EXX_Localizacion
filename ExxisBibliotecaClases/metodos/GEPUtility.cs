using ExxisBibliotecaClases.entidades;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExxisBibliotecaClases.metodos
{
    public class GEPUtility
    {
        private static log4net.ILog logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public static FormatoElectronico Load(string strUrl)
        {
            FormatoElectronico f = null;
            try
            {
                byte[] bytes = TripleDESCryptoHelper.DecryptFromFile(strUrl);
                string archivoxml = UnZip(bytes);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(archivoxml);
                f = new FormatoElectronico()
                {
                    Name = xmlDocument.DocumentElement.Attributes["name"].Value,
                    FileName = Path.GetFileName(strUrl)
                };
                if (File.Exists(archivoxml)) File.Delete(archivoxml);
            }
            catch (Exception ex)
            {
                logger.Error("Open", ex);
            }
            return f;
        }
        public static FormatoElectronico LoadFromStream(Stream stream)
        {
            FormatoElectronico f = null;
            try
            {
                //creando archivo temporal
                string filetemp = Path.GetTempFileName();
                using (FileStream fs = File.Create(filetemp))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fs);
                }
                f = Load(filetemp);
                f.Content = stream;
                if (File.Exists(filetemp)) File.Delete(filetemp);
            }
            catch (Exception ex)
            {
                logger.Error("Open", ex);
            }
            return f;
        }
        private static string UnZip(byte[] bytes)
        {
            string FlowFileName = "Flow.xml";
            string ZipFileName = "EFM.zip";
            string dir = $"{Path.GetTempPath()}EX_LOC\\ZIP";

            string ZipFilePath = $"{dir}\\{ZipFileName}";
            string FlowFilePath = $"{dir}\\{FlowFileName}";

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            FileStream fileStream = File.Open(ZipFilePath, FileMode.Create);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
            ZipFile zf = new ZipFile(ZipFilePath);
            zf.ExtractSelectedEntries(FlowFileName, null, dir);
            zf.Dispose();
            if (File.Exists(ZipFilePath)) File.Delete(ZipFilePath);
            return FlowFilePath;
        }
    }
}
