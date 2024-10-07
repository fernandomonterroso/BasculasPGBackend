using BasculasPG.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.IO.Ports;

namespace BasculasPG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasculaController : ControllerBase
    {
        [HttpGet("v2")]
        public IActionResult GetPeso([FromQuery(Name = "port")] string port, [FromQuery(Name = "command")] string command)
        {
            string devuelve="";
            Peso myObj = new Peso();
            Error modelError = new Error();
            try
            {

                if (port == null || command == null)
                {
                    throw new ArgumentNullException("faltan parametros.");
                }

                SerialPort port1 = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
                bool serialResponse = false;
                string accumulated = "";
                port1.Open();
                port1.Write(command + (char)13 + (char)10);
                DateTime moment = DateTime.Now;
                string weight = "";
                port1.DataReceived += new SerialDataReceivedEventHandler(
                    delegate (object sender, SerialDataReceivedEventArgs e)
                    {
                        var inputData = port1.ReadExisting();
                        accumulated += inputData;
                        if (accumulated.Contains("kg") || accumulated.Contains("lb"))
                        {
                            string[] data = accumulated.Split('\n');

                            port1.Close();
                            var lineWithWeight = data.Where(s => s.Contains("kg") || s.Contains("lb")).ToList();
                            if (lineWithWeight.Count <= 0)
                            {
                                weight = "-1 Kg";
                            }
                            else
                            {
                                weight = lineWithWeight[0];
                            }
                            //Console.WriteLine(weight.Trim());

                            accumulated = "";
                            devuelve = weight.Trim();
                            serialResponse = true;
                        }
                    });
                while (true)
                {
                    if (serialResponse)
                    {
                        //Console.WriteLine("DEVOLVIENDOS: " +devuelve);
                        port1.Close();
                        string limpiar = devuelve.Replace("\t", "");
                        while (limpiar.IndexOf("  ") >= 0)
                        {
                            limpiar = limpiar.Replace("  ", "");
                            limpiar = limpiar.Replace("PESO", "");
                        }
                        myObj.peso = limpiar;
                        return Ok(myObj);
                        break;
                    }
                    if (moment.AddSeconds(10) <= DateTime.Now)
                    {
                        if (port1.IsOpen)
                        {
                            port1.Close();
                        }
                        //Console.WriteLine("dd: "+moment);
                        //
                        throw new TimeoutException("Se excedio el tiempo de espera para leer el puerto serial.");
                        break;
                    }
                }


            }
            catch (Exception e)
            {
                modelError.error = e.Message;
                return BadRequest(modelError);
            }
            //myObj.peso = "line;
            

        }

        [HttpGet("v1")]
        public IActionResult GetRandomWeight()
        {
            var random = new Random();
            decimal randomWeight = Math.Round((decimal)(random.NextDouble() * (1500 - 1) + 1), 2);
            return Ok(new Peso { peso = randomWeight.ToString() + " Kg" });
        }
    }
}
