using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Microsoft.Reporting.WinForms;
using System.Drawing;
using System.Drawing.Imaging;

namespace ServicioTecnicoReporte
{
    class Task
    {
        string codigocliente;
        public void GenerateReports()
        {
            
            //int reporteID = 1651653068;
            DataTable recordToProcessDT = new DataTable();
            do
            {
                string reporteId ="";
                string Serie = "";
                try
                {
                    string connString = "ApplicationConnectionString";
                    string sp = "LECT_SelectRPTtoProcessCargaMasiva";
                    Dictionary<string, object> Parameters = new Dictionary<string, object>();
                    recordToProcessDT = DAL_Sql.GetDataByStoreProc(connString, sp, Parameters);
                    foreach (DataRow row in recordToProcessDT.Rows)
                    {

                       
                        bool follow = false;


                        if (!row.IsNull("Num_Documento"))
                        {
                            reporteId = Convert.ToString(row["Num_Documento"]);
                            Serie = Convert.ToString(row["Serie"]);
                        }
                        if (reporteId != "")
                        {
                            follow = true;
                        }
                        //Generar PDF
                        string archivoPdf = "";
                        string pathPDF = Configuracion.GetConfiguracion("Ruta Reporte");
                        if (follow)
                        {
                            // Generar PDF
                            archivoPdf = GeneratePdfReporte(pathPDF, reporteId, Serie);
                            Console.WriteLine("PDF creado" + reporteId);
                            follow = !String.IsNullOrEmpty(archivoPdf);
                        }
                        //Actualizar Ruta Archivo
                        if (follow)
                        {
                            follow = UpdateRutaArchivo(reporteId, archivoPdf, Serie);
                        }
                        //Actualizar Estado
                        if (follow)
                        {
                            follow = CambiarEstadoReporte(reporteId, 2, Serie);
                        }

                        //Enviar Email
                        if (follow)
                        {

                            EMail mail = new EMail();
                            EmailRes resEm = new EmailRes();
                            string SmtpServer = Configuracion.GetConfiguracion("SMTP Server");
                            string FromMail = Configuracion.GetConfiguracion("From Email");
                            string NameMail = Configuracion.GetConfiguracion("Nombre Email");
                            string Asunto = Configuracion.GetConfiguracion("Correo Asunto");
                            string CuerpoMensaje = Configuracion.GetConfiguracion("Correo Cuerpo");
                            DataTable EncabezadoDT = this.EncabezadoReporte(reporteId, Serie);
                            foreach (DataRow rowReporte in EncabezadoDT.Rows)
                            {
                                if (!rowReporte.IsNull("Modelo"))
                                {

                                    CuerpoMensaje = CuerpoMensaje.Replace("{Modelo}", rowReporte["Modelo"].ToString());
                                }

                                if (!rowReporte.IsNull("Serie"))
                                {
                                    Asunto = Asunto.Replace("{Serie}", rowReporte["Serie"].ToString());
                                    CuerpoMensaje = CuerpoMensaje.Replace("{Serie}", rowReporte["Serie"].ToString());
                                }

                                string ToMail = "";
                                string ToName = "";
                                if (!rowReporte.IsNull("Email"))
                                {
                                    ToMail = rowReporte["Email"].ToString();

                                }

                                else
                                {
                                    ToMail = "mvargas@canella.com.gt";
                                }

                                if (!rowReporte.IsNull("NombreContacto"))
                                {
                                    ToName = rowReporte["NombreContacto"].ToString();
                                    CuerpoMensaje = CuerpoMensaje.Replace("{Cliente}", ToName);
                                }

                                else
                                {
                                    ToName = "A quien interese";
                                    CuerpoMensaje = CuerpoMensaje.Replace("{Cliente}", ToName);
                                }

                                if (!String.IsNullOrEmpty(ToMail) && !String.IsNullOrEmpty(ToName))
                                {
                                    follow = mail.EnviarCorreoInternoCanella(FromMail, NameMail, SmtpServer, ToMail, Asunto,
                                        CuerpoMensaje, pathPDF + "\\" + archivoPdf);


                                }
                            }

                        }

                        if (follow)
                        {
                            CambiarEstadoReporte(reporteId, 3, Serie);
                           
                        }
                        else {
                            CambiarEstadoReporte(reporteId, 4, Serie);
                           
                        }

                      


                    }

                    Console.WriteLine("Reporte creado " + reporteId);
                }
                catch (Exception ex) {
                    Console.WriteLine("No pudo procesar reporteid:" + reporteId + " Error:" + ex.Message);
                }
            } 
            
            while (recordToProcessDT.Rows.Count > 0);

            Console.WriteLine("No hay mas registros para procesar");
           

        }

        public DataTable EncabezadoReporte(string reporteId, string Serie)
        {
            DataTable dt = new DataTable();
            try
            {
                string connString = "ApplicationConnectionString";
                string sp1 = "LECT_SelectRPTEncabezadoPDF";
                Dictionary<string, object> Parameters = new Dictionary<string, object>();
                Parameters.Add("@IdReporte", reporteId);
                Parameters.Add("@Serie", Serie);
                dt = DAL_Sql.GetDataByStoreProc(connString, sp1, Parameters);
            }
            catch (Exception ex) {
                UpdateRutaArchivo(reporteId, Convert.ToString(ex), Serie);
            }
            return dt;
        }
        

        public bool CambiarEstadoReporte(string reporteId, int ReporteEstadoID, string Serie)
        {
            bool res = false;
            try
            {
                string connString = "ApplicationConnectionString";
                string sp1 = "LECT_UpdateEstadoReporte";
                Dictionary<string, object> Parameters = new Dictionary<string, object>();
                Parameters.Add("@IdReporte", reporteId);
                Parameters.Add("@ReporteEstadoID", ReporteEstadoID);
                Parameters.Add("@Serie", Serie);
                res = DAL_Sql.ExecuteDataByStoreProc(connString, sp1, Parameters);
            }
            catch (Exception ex) { }

            return res;
        }

        public bool UpdateRutaArchivo(string reporteId, string ArchivoReporte, string Serie)
        {
            bool res = false;
            try
            {
                string connString = "ApplicationConnectionString";
                string sp1 = "LECT_UpdateArchivoReporte";
                Dictionary<string, object> Parameters = new Dictionary<string, object>();
                Parameters.Add("@IdReporte", reporteId);
                Parameters.Add("@ArchivoReporte", ArchivoReporte);
                Parameters.Add("@Serie", Serie);
                res = DAL_Sql.ExecuteDataByStoreProc(connString, sp1, Parameters);
            }
            catch (Exception ex) { }

            return res;
        }

        public string GeneratePdfReporte(string pathPDF, string reporteId, string serie)
        {
            string reportePdf = "";

            string mesanio = "";

            try
            {
                string connString = "ApplicationConnectionString";
                Dictionary<string, object> Parameters = new Dictionary<string, object>();
                DataTable EncabezadoDT = this.EncabezadoReporte(reporteId, serie);
                EncabezadoDT.Columns.Add("ContieneFirma", typeof(String));
                EncabezadoDT.Columns.Add("ImagenFirma", typeof(Byte[]));

                foreach (DataRow row in EncabezadoDT.Rows)
                {
                    row["ContieneFirma"] = "No";
                    if (!row.IsNull("firma"))
                    {
                        try
                        {
                            string base64String = row["firma"].ToString();
                            var bytesFirma = Convert.FromBase64String(base64String);
                            MemoryStream streamIM64 = new MemoryStream(bytesFirma);
                            //var contents = new StreamContent(new MemoryStream(bytesFirma));
                            Compression compression = new Compression();
                            Bitmap bitmap = new Bitmap(streamIM64);
                           // bitmap = compression.ImageNewSize(bitmap, ancho, largo);
                            row["ImagenFirma"] = ImageExtensions.ToByteArray(bitmap, ImageFormat.Png);
                            row["ContieneFirma"] = "Si";
                            codigocliente = row["CodigoCliente"].ToString();
                            serie = row["Serie"].ToString();

                        }
                        catch (Exception ex) {
                            UpdateRutaArchivo(reporteId, "Enc" + Convert.ToString(ex), serie);
                        }
                    }

                    row["ContieneFirma"] = "Si";
                    codigocliente = row["CodigoCliente"].ToString();
                    serie = row["Serie"].ToString();

                }

                mesanio = Convert.ToString(DateTime.Now.Month) + "_" + Convert.ToString(DateTime.Now.Year);

                string sp2 = "LECT_SelectRPT_DetallePDF";
                Parameters = new Dictionary<string, object>();
                Parameters.Add("@IdReporte", reporteId);
                DataTable PartesDT = DAL_Sql.GetDataByStoreProc(connString, sp2, Parameters);



                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                ReportViewer ReportViewer1 = new ReportViewer();
                ReportViewer1.ProcessingMode = ProcessingMode.Local;
                ReportViewer1.LocalReport.EnableExternalImages = true;
                //set path of the Local report  
                //path pruebas
                //ReportViewer1.LocalReport.ReportPath = "C:\\CorreoLecturasPDF\\ReporteServicio.rdlc";
                //path produccion
                ReportViewer1.LocalReport.ReportPath = "C:\\LecturasRPT\\ReporteServicio.rdlc";

                //ReportViewer1.LocalReport.

                ReportViewer1.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("Encabezado", EncabezadoDT);
                ReportDataSource rds2 = new ReportDataSource("Partes", PartesDT);
                //Add ReportDataSource  
                ReportViewer1.LocalReport.DataSources.Add(rds);
                ReportViewer1.LocalReport.DataSources.Add(rds2);
                ReportViewer1.LocalReport.Refresh();
                ReportViewer1.RefreshReport();


                byte[] bytes;
                if (ReportViewer1.ProcessingMode == ProcessingMode.Local)
                {
                    bytes = ReportViewer1.LocalReport.Render("PDF", null, out mimeType,
                     out encoding, out filenameExtension, out streamids, out warnings);
                }
                else
                {
                    bytes = ReportViewer1.ServerReport.Render("PDF", null, out mimeType,
                     out encoding, out filenameExtension, out streamids, out warnings);
                }
              //= ReportViewer1.LocalReport.Render(
              //  "PDF", null, out mimeType, out encoding, out filenameExtension,
              //  out streamids, out warnings);
              //  //reportePdf = Guid.NewGuid().ToString() + ".pdf";
                reportePdf =  reporteId+"_"+serie+"_"+ codigocliente + "_"+mesanio+ ".pdf";
                using (FileStream fs = new FileStream(pathPDF + "\\" + reportePdf, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }

            }
            catch (Exception ex) {
                reportePdf = "";
                string[] partes =  Convert.ToString(ex).Split('>');

                if (partes.Length >= 1)
                {
                    string resultado = partes[1];
                    UpdateRutaArchivo(reporteId,  resultado, serie);
                }
                Console.WriteLine(reportePdf + "  "+ ex);
                    //UpdateRutaArchivo(reporteId,"Det"+ resultado, serie);
            }
            return reportePdf;
        }
    }
}
