using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Http;
using System.Drawing.Imaging;

namespace ServicioTecnicoReporte
{
    class Program
    {
        static void Main(string[] args)
        {      
            Task task = new Task();
            task.GenerateReports();
          //task.GeneratePdfReporte(Configuracion.GetConfiguracion("Ruta Reporte"), 15);
        }
    }
}
