using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Proye3JoseDRodriguezSemestreI2019.Models;

namespace Proye3JoseDRodriguezSemestreI2019.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //Variable necesaria para la conexion a db
        public IConfiguration Configuration { get; }

        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }//Constructor con archivo de conexion

            public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult cargarVistaDestinos()
        {
            List<Destino> lista_destinos = new List<Destino>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_listar_destinos";
                SqlDataReader reader;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                    Destino destino;
                    while (reader.Read())
                    {
                        destino = new Destino();
                        destino.nombre_destino = reader[0].ToString();
                        destino.descripcion = reader[1].ToString();
                        lista_destinos.Add(destino);
                    }

                }
                connection.Close();
            }

            foreach (Destino d in lista_destinos)
            {
                System.Diagnostics.Debug.WriteLine(d.nombre_destino);
                System.Diagnostics.Debug.WriteLine(d.descripcion);
            }

            //HttpContext.Session.SetString("destinos",lista_destinos[0]);
            ViewBag.DestinosInfo = lista_destinos;
            return View("DestinosView");
        }

        public IActionResult cargarVistaConsejos()
        {
            return View("ConsejosView");
        }

    }
}
