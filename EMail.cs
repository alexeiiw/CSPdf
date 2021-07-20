using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.IO;

namespace ServicioTecnicoReporte
{
    public  class EMail
    {
 

        public  bool EnviarCorreoInternoCanella(
            string fromEmail, string NameEmail, string SmtpServer,
            string ToMail,  string SubjectMail, string BodyMail, string fileName = "")

        {


            bool res = false;
            try
            {

                string ServidorCorreoCanella = SmtpServer;



                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(fromEmail, NameEmail);

                mail.To.Add(ToMail);
                //mail.CC.Add("lecturas@canella.com.gt");
                //mail.CC.Add("fhernandez@canella.com.gt");
                mail.CC.Add("mvargas@canella.com.gt");
                // mail.To.Add("atobar@canella.com.gt");

                mail.Subject = SubjectMail;

                mail.Body = BodyMail;

                mail.IsBodyHtml = true;

                //SmtpClient smpt = new SmtpClient("128.1.200.141");

                SmtpClient smpt = new SmtpClient(ServidorCorreoCanella);

                //            smpt.Port = 25;

                //            smpt.UseDefaultCredentials = true;

                //            smpt.Timeout = 25;

                if (!String.IsNullOrEmpty(fileName))
                {
                    mail.Attachments.Add(new Attachment(fileName));
                }

                smpt.Send(mail);
                res = true;
                Console.WriteLine("Correo Enviado Exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo. Error: " + ex.Message);
            }
            return res;

            return true;

        }



        public bool sendEmail_IndividualGmail(string fromEmail, string NameEmail, string SmtpServer,
            string toEmaill,  string asunto, string mensaje,  string fileName="" )

           
            
        {

            fromEmail = "cuenta@gmail.com";
            string fromPassword = "pass";
            bool res = false;
            string excepcionMensaje = "";
            try
            {
                var fromAddress = new MailAddress(fromEmail, NameEmail);
                var toAddress = new MailAddress(toEmaill);
                
                string subject = asunto;
                string body = mensaje;
                bool enableSSL = true;
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = enableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout= 10000,
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.EnableSsl = enableSSL;
                    if (!String.IsNullOrEmpty(fileName))
                    {
                        message.Attachments.Add(new Attachment(fileName));
                    }
                    smtp.Send(message);
                    res = true;
                }
            }
            catch (Exception ex)
            {
                excepcionMensaje = ex.Message;
                //   ExceptionManager.LogExceptionStackTraceToDB(ex.Message, ex.StackTrace, "correo " + toEmaill, "Tarea: Envio correo");

            }

            return res;
        }
    }

    public class EmailRes {
        public string ExcepcionMensaje { get; set; }
        public bool ResEmailSent { get; set; }
    }
}