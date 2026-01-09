using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace exxis_localizacion.google
{
    public class GoogleService
    {
        #region Variables

        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly, SheetsService.Scope.DriveReadonly };
        static string ApplicationName = "exxis_localizacion";
        string google_client;
        string error;

        #endregion
                
        public SheetsService getGoogle_Proxy_SheetService()
        {
            try
            {
                UserCredential credential = getCredentials();

                // Create Google Sheets API service.
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                return service;

            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return null;
        }
        public DriveService getGoogle_Proxy_Service()
        {
            try
            {
                UserCredential credential = getCredentials();

                // Create Google Sheets API service.
                DriveService service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                return service;

            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return null;
        }

        private UserCredential getCredentials()
        {
            google_client = "resources\\client_secret.json";

            string paths = AppDomain.CurrentDomain.BaseDirectory;
            google_client = paths + google_client;

            UserCredential credential;

            using (var stream = new FileStream(google_client, FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, $".credentials/sheets.googleapis.com.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            }

            return credential;
        }
    }
}
