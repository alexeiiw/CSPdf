using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ServicioTecnicoReporte
{
    public static class Configuracion
    {
        public static string GetConfiguracion(string Nombre)
        {
            string res = "";
            try
            {
                string connString = "ApplicationConnectionString";
                string sp = "LECT_SelectConfiguracion";
                Dictionary<string, object> Parameters = new Dictionary<string, object>();
                Parameters.Add("@Nombre", Nombre);
                DataTable recordToProcessDT = DAL_Sql.GetDataByStoreProc(connString, sp, Parameters);
                foreach (DataRow row in recordToProcessDT.Rows)
                {
                    if (!row.IsNull("Valor"))
                    {
                        res = row["Valor"].ToString();
                    }
                }
            }
            catch (Exception) { }

            return res;
        }

    }
}
