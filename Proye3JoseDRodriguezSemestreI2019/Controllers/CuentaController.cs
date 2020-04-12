using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Proye3JoseDRodriguezSemestreI2019.Models;
using Microsoft.AspNetCore.Http; // Se incluye para poder hacer uso del session
using System.Net.Mail;
using System.Net;

namespace Proye3JoseDRodriguezSemestreI2019.Controllers
{
    public class CuentaController : Controller
    {
        //Variable necesaria para la conexion a db
        public IConfiguration Configuration { get; }

        public CuentaController(IConfiguration configuration)
        {
            Configuration = configuration;
        }//Constructor con archivo de conexion
        
        public ActionResult CrearCuenta()
        {
            return View("CrearCuentaView");
        }//retorna la vista principal de crear cuenta

        
        public ActionResult IniciarSesionAdmin()
        {
            return View("IniciarSesionAdminView");
        }//retorna la vista principal de iniciar sesion como admin

        public ActionResult cargarVistaCrearAdmin()
        {
            return View("CrearAdminView");
        }//retorna la vista principal de crear admin

        public ActionResult cargarVistaCrearVuelos()
        {
            List<String> lista_destinos = new List<String>();
            List<String> lista_aviones = new List<String>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_obtener_destinos";
                string sql2 = $"EXECUTE sp_obtener_aviones";
                SqlDataReader reader;
                SqlDataReader reader2;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                       lista_destinos.Add(reader[0].ToString());
                    }
                    
                }
                using (SqlCommand command = new SqlCommand(sql2, connection))
                {
                    command.CommandType = CommandType.Text;
                    reader2 = command.ExecuteReader();
                    while (reader2.Read())
                    {
                        lista_aviones.Add(reader2[0].ToString());
                    }

                }
                connection.Close();
            }
            
            //HttpContext.Session.SetString("destinos",lista_destinos[0]);
            ViewBag.Destinos = lista_destinos;
            ViewBag.Aviones = lista_aviones;
            return View("CrearVuelosView");
        }//retorna la vista principal de crear vuelos

        //Metodo usado para iniciar sesion en el modulo administrativo
        [HttpPost]
        public ActionResult Entrar(Administrador a)
        {
            int resultado = 0;
            string vista = "";
            System.Diagnostics.Debug.WriteLine(a.nombre_usuario);
            System.Diagnostics.Debug.WriteLine(a.contrasena);
            if (ModelState.IsValid)
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"EXECUTE sp_inicio_sesion_admin @param_nombre = '{a.nombre_usuario}', @param_contrasena = '{a.contrasena}'";
                    SqlDataReader reader;
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            resultado = reader.GetInt32(0);
                        }
                        connection.Close();
                    }
                }
            }

            if (resultado == 1)
            {
                System.Diagnostics.Debug.WriteLine("----------------------->LOGEADO");
                vista = "AdminView";
                HttpContext.Session.SetString("nombreAdmin",a.nombre_usuario);
                
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
        }


        [HttpPost]
        public ActionResult crearNuevoAdmin(Administrador a)
        {
            int resultado = 0;
            string vista = "";
            string nombre = a.nombre_usuario;
            string contrasena = a.contrasena;
            System.Diagnostics.Debug.WriteLine("nombre "+nombre);
            System.Diagnostics.Debug.WriteLine("contrasena "+contrasena);
            if (ModelState.IsValid)
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"EXECUTE sp_crear_admin @param_nombre = '{a.nombre_usuario}', @param_contrasena = '{a.contrasena}'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        resultado = command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }

            if (resultado == 1)
            {
                vista = "AdminView";
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
        }


        /*Metodo que genera el codigo de vuelo de acuerdo a la inicial del lugar de salida y del lugar de destino,
        seguido de un random de 4 digitos*/
        [HttpPost]
        public string generarCodigoVuelo(string select_salida, string select_destino)
        {
            string codido = select_salida[0].ToString() + select_destino[0].ToString();
            System.Diagnostics.Debug.WriteLine(codido);
            Random random = new Random();
            int n = random.Next(1000, 9999);
            codido = codido + n;
            return codido;
        }

        [HttpPost]
        public ActionResult crearVuelo(Vuelo v)
        {
            int resultado = 0;
            int resultado2 = 0;
            string vista = "";
            string codigo = v.codigo;
            string lugar_salida = v.lugar_salida;
            string destino = v.destino;
            string fecha_salida = v.fecha_salida;
            string fecha_llegada = v.fecha_llegada;
            string avion = v.avion;

            System.Diagnostics.Debug.WriteLine(codigo,lugar_salida+destino+fecha_salida+fecha_llegada+avion);

            if (ModelState.IsValid)
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"EXECUTE sp_crear_vuelo @param_codigo = '{v.codigo}',@param_lugar_salida = '{v.lugar_salida}', @param_destino = '{v.destino}', @param_fecha_salida = '{v.fecha_salida}',@param_fecha_llegada = '{v.fecha_llegada}',@param_avion = '{v.avion}',@asientos = {15}";
                    string sql2 = $"EXECUTE insertar_vuelo_asientos @codigo_vuelo = '{v.codigo}'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        resultado = command.ExecuteNonQuery();
                        
                    }
                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {
                        command.CommandType = CommandType.Text;
                        resultado2 = command.ExecuteNonQuery();
                        
                    }
                    connection.Close();
                    
                }
            }

            if (resultado == 1 && resultado2 == 15)
            {
                vista = "AdminView";
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
        }

        public ActionResult cargarVistaCrearDescuentos()
        {
            return View("CrearDescuentosView");
        }//retorna la vista principal de crear descuentos

        [HttpPost]
        public IActionResult crearDescuentos(Descuento d)
        {
            int resultado = 0;
            string vista = "";
            if (ModelState.IsValid)
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"EXECUTE sp_crear_descuento @param_codigo = '{d.codigo}', @param_valor = {d.porcentaje_descuento}";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        resultado = command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }

            if (resultado == 1)
            {
                
                vista = "AdminView";
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
        }


        public IActionResult cargarVistaVentas() {

            List<Venta> lista_ventas = new List<Venta>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"SELECT * FROM VENTAS";
                SqlDataReader reader;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                    Venta venta;
                    while (reader.Read())
                    {
                        venta = new Venta();
                        venta.correo = Convert.ToString(reader["correo_cliente"]);
                        venta.total = Convert.ToString(reader["total"]);
                        venta.reserva = Convert.ToString(reader["codigo_reserva"]);
                        lista_ventas.Add(venta);
                    }
                    connection.Close();

                }
            }
            return View("VentasView",lista_ventas);
        }

        


        /**********************CLIENTES**************************/

        [HttpPost]
        public IActionResult crearCuentaCliente(Cliente c)
        {
            int resultado = 0;
            string vista = "";
            if (ModelState.IsValid)
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"EXECUTE sp_crear_cliente @param_nombre = '{c.nombre_cliente}', @param_contrasena = '{c.contrasena}', @param_correo = '{c.correo}'";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        resultado = command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }

            if (resultado == 1)
            {
                HttpContext.Session.SetString("userEmail",c.correo);
                vista = "~/Views/Cliente/ClienteIndexView.cshtml";
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
        }

        public IActionResult cerrarSesion()
        {
            return View("~/Views/Home/Index.cshtml");
        }


        [HttpPost]
        public IActionResult iniciarSesionCliente(Cliente c)
        {
            int resultado = 0;
            string vista = "";
            if (ModelState.IsValid)
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"EXECUTE sp_iniciar_sesion_cliente @param_correo = '{c.correo}', @param_contrasena = '{c.contrasena}'";
                    SqlDataReader reader;
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            resultado = reader.GetInt32(0);
                        }
                        connection.Close();
                    }
                }
            }

            if (resultado == 1)
            {
                HttpContext.Session.SetString("userEmail", c.correo);
                vista = "~/Views/Cliente/ClienteIndexView.cshtml";
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
        }

        public void prueba()
        {

        }
    
    }
}