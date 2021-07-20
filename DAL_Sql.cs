using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ServicioTecnicoReporte
{
    public static class DAL_Sql
    {
        public static DataTable GetDataByStoreProc(string ConnString, string StoreProc, Dictionary<string, object> Params)
        {
            DataTable dtRes = new DataTable();
            SqlConnection con = new SqlConnection();
            try
            {
                string connString = ConfigurationManager.ConnectionStrings[ConnString].ConnectionString;
                con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(StoreProc, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                foreach (string ParamName in Params.Keys)
                {
                    cmd.Parameters.Add(new SqlParameter(ParamName, Params[ParamName]));
                }
                        
                con.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dtRes);
                con.Close();

            }
            catch (Exception ex)
            {
                // Log exception

            }
            finally
            {

                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            return dtRes;
        }


        public static bool ExecuteDataByStoreProc(string ConnString, string StoreProc, Dictionary<string, object> Params)
        {
            bool res = false;
            SqlConnection con = new SqlConnection();
            try
            {
                string connString = ConfigurationManager.ConnectionStrings[ConnString].ConnectionString;
                con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(StoreProc, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (string ParamName in Params.Keys)
                {
                    cmd.Parameters.Add(new SqlParameter(ParamName, Params[ParamName]));
                }
                  
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                res = true;

            }
            catch (Exception ex)
            {
                // Log exception

            }
            finally
            {

                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            return res;
        }


        public static bool ExecuteDataByStoreProc_Output(string ConnString, string StoreProc, 
            Dictionary<string, object> Params, string outputParamName, SqlDbType type, out object val )
        {
            bool res = false;
            val = null;
            SqlConnection con = new SqlConnection();
            try
            {
                string connString = ConfigurationManager.ConnectionStrings[ConnString].ConnectionString;
                con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand(StoreProc, con);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (string ParamName in Params.Keys)
                {
                    cmd.Parameters.Add(new SqlParameter(ParamName, Params[ParamName])).Direction= ParameterDirection.Input;
                }
                cmd.Parameters.Add(new SqlParameter(outputParamName, type)).Direction = ParameterDirection.Output;

               con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                val = cmd.Parameters[outputParamName].Value;
                res = true;

            }
            catch (Exception ex)
            {
                // Log exception

            }
            finally
            {

                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            return res;
        }
    }
}
