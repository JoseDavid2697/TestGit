using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Proye3JoseDRodriguezSemestreI2019.Models;
using X.PagedList;
namespace Proye3JoseDRodriguezSemestreI2019.Controllers
{
    public class ClienteController : Controller
    {

        //Variable necesaria para la conexion a db
        public IConfiguration Configuration { get; }

        public ClienteController(IConfiguration configuration)
        {
            Configuration = configuration;
        }//Constructor con archivo de conexion

        // GET: Cliente
        public ActionResult Index()
        {
            return View();
        }


        //Carga la vista para buscar vuelos
        public IActionResult cargarVistaReservarVuelo()
        {
            List<String> lista_destinos = new List<String>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_obtener_destinos";
                SqlDataReader reader;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista_destinos.Add(reader[0].ToString());
                    }
                    connection.Close();

                }
            }

            ViewBag.DestinosCliente = lista_destinos;
            return View("ReservarVueloView");
        }


        //Realiza la busqueda de vuelos
        public IActionResult ListarVuelosView(int? page)
        {
            int size = 4;
            int pageIndex = 1;
            pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            List<Vuelo> lista_vuelos = new List<Vuelo>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_listar_vuelos";
                SqlDataReader reader;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                    Vuelo vuelo;
                    while (reader.Read())
                    {
                        vuelo = new Vuelo();
                        vuelo.codigo = Convert.ToString(reader["id"]);
                        vuelo.lugar_salida = Convert.ToString(reader["lugar_salida"]);
                        vuelo.destino = Convert.ToString(reader["destino"]);
                        vuelo.fecha_salida = Convert.ToString(reader["fecha_salida"]);
                        vuelo.fecha_llegada = Convert.ToString(reader["fecha_llegada"]);
                        vuelo.avion = Convert.ToString(reader["avion"]);
                        vuelo.asientos = Convert.ToString(reader["cantidad_asientos"]);
                        lista_vuelos.Add(vuelo);
                    }
                    connection.Close();

                }
            }

            IPagedList vuelos = lista_vuelos.ToPagedList(pageIndex, size);
            return View(vuelos);
        }


        [HttpPost]
        public IActionResult cargarVistaLlenarReserva(string codigo_vuelo_elegido)
        {
            System.Diagnostics.Debug.WriteLine("Codigo: "+codigo_vuelo_elegido);
            ViewData["codigo_vuelo_elegido"] = codigo_vuelo_elegido;
            return View("LlenarReservaView");
        }


        /*Metodo que genera el codigo de reserva*/
        [HttpPost]
        public string generarCodigoReserva()
        {
            string codido = "RE";
            System.Diagnostics.Debug.WriteLine(codido);
            Random random = new Random();
            int n = random.Next(1000, 9999);
            codido = codido + n;
            return codido;
        }

        public IActionResult realizarPago(string tipo_boleto,string cantidad_asientos,string clase,string codigo_reserva, string codigo_vuelo,string correo_cliente)
        {
            
            int precio = 0;
            if (clase.Equals("Económica"))
            {
                precio = Convert.ToInt32(cantidad_asientos) * 100;
            }
            else if(clase.Equals("Media"))
            {
                precio = Convert.ToInt32(cantidad_asientos) * 200;
            }
            else if(clase.Equals("Premium"))
            {
                precio = Convert.ToInt32(cantidad_asientos) * 300;
            }

            if (tipo_boleto.Equals("Round-Trip"))
            {
                double descuento = precio * 0.05;
                precio = Convert.ToInt32(precio - descuento);
            }

            System.Diagnostics.Debug.WriteLine(tipo_boleto + cantidad_asientos + clase + codigo_reserva + codigo_vuelo + correo_cliente + precio);
            HttpContext.Session.SetString("tipo_boleto_correo", tipo_boleto);
            HttpContext.Session.SetString("cantidad_asientos_correo", cantidad_asientos);
            HttpContext.Session.SetString("clase_correo", clase);

            HttpContext.Session.SetString("codigo_reserva_correo", codigo_reserva);
            HttpContext.Session.SetString("codigo_vuelo_correo", codigo_vuelo);

            HttpContext.Session.SetString("correo_cliente_correo", correo_cliente);
            HttpContext.Session.SetString("precio_correo", precio.ToString());


            //Inserta en tabla reservas
            int resultado = 0;
            string vista = "";

            string tip = HttpContext.Session.GetString("tipo_boleto_correo");
            string cant = HttpContext.Session.GetString("cantidad_asientos_correo");
            string clas = HttpContext.Session.GetString("clase_correo");
            string cr = HttpContext.Session.GetString("codigo_reserva_correo");
            string cv = HttpContext.Session.GetString("codigo_vuelo_correo");
            string cc = HttpContext.Session.GetString("correo_cliente_correo");
            string pre = HttpContext.Session.GetString("precio_correo");


            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_insertar_reserva @param_tipo = '{tip}', @param_cantidad = '{cant}',@param_clase = '{clas}',@param_codigo = '{cr}',@param_vuelo = '{cv}',@param_correo = '{cc}',@param_precio = '{pre}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    resultado = command.ExecuteNonQuery();
                    connection.Close();

                }
            }

            if (resultado == 1)
            {
                vista = "RealizarPagoView";
            }
            else
            {
                vista = "Error";
            }

            //Envia el correo de aviso
            this.SendEmail(codigo_reserva, precio.ToString(), correo_cliente);


            return View(vista);
        }

        public IActionResult realizarPagoFinal()
        {
            int resultado = 0;
            string vista = "";

            string correo = HttpContext.Session.GetString("correo_cliente_correo");
            string total = HttpContext.Session.GetString("precio_correo");
            string reserva = HttpContext.Session.GetString("codigo_reserva_correo");

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_insertar_venta @param_correo = '{correo}', @param_total = '{total}',@param_reserva = '{reserva}'";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    resultado = command.ExecuteNonQuery();
                    connection.Close();

                }
            }
       

            if (resultado == 1)
            {
                vista = "~/Views/Cliente/ClienteIndexView.cshtml";
            }
            else
            {
                vista = "Error";
            }

            return View(vista);
            
        }


        //Metodo que envia un correo al cliente

        public ActionResult SendEmail(string codigo_reserva, string precio,string correo_cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var senderEmail = new MailAddress("josedavid2697@gmail.com", "Jose David");
                    var receiverEmail = new MailAddress(correo_cliente, "Cliente");
                    var password = "...";
                    var sub = "Compra de Boleto UCR AIRLINES";
                    var body = "Su compra se ha realizado con éxito, su código de reserva es: "+codigo_reserva+", el precio total es de: $"+precio;
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = sub,
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }
                    return View("~/Views/Home/Index.cshtml");
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Some Error";
            }
            return View("~/Views/Home/Index.cshtml");
        }



        public IActionResult carrito()
        {
            List<Carrito> lista_carrito = new List<Carrito>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_listar_carrito @param_correo = '{HttpContext.Session.GetString("userEmail")}'";
                SqlDataReader reader;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                    Carrito carrito;
                    while (reader.Read())
                    {
                        carrito = new Carrito();
                        carrito.correo = Convert.ToString(reader["correo"]);
                        carrito.codigo_reserva = Convert.ToString(reader["codigo_reserva"]);
                        carrito.total = Convert.ToString(reader["total"]);
                        lista_carrito.Add(carrito);
                    }
                    connection.Close();

                }
            }
            return View("CarritoView", lista_carrito);
        }



        //Agregar al carrito
        [HttpPost]
        public string agregarAlCarrito(string tipo_boleto, string cantidad_asientos, string clase, string codigo_reserva, string codigo_vuelo, string correo_cliente)
        {

            int precio = 0;
            if (clase.Equals("Económica"))
            {
                precio = Convert.ToInt32(cantidad_asientos) * 100;
            }
            else if (clase.Equals("Media"))
            {
                precio = Convert.ToInt32(cantidad_asientos) * 200;
            }
            else if (clase.Equals("Premium"))
            {
                precio = Convert.ToInt32(cantidad_asientos) * 300;
            }

            if (tipo_boleto.Equals("Round-Trip"))
            {
                double descuento = precio * 0.05;
                precio = Convert.ToInt32(precio - descuento);
            }

            System.Diagnostics.Debug.WriteLine(tipo_boleto + cantidad_asientos + clase + codigo_reserva + codigo_vuelo + correo_cliente + precio);
            HttpContext.Session.SetString("tipo_boleto_correo", tipo_boleto);
            HttpContext.Session.SetString("cantidad_asientos_correo", cantidad_asientos);
            HttpContext.Session.SetString("clase_correo", clase);

            HttpContext.Session.SetString("codigo_reserva_correo", codigo_reserva);
            HttpContext.Session.SetString("codigo_vuelo_correo", codigo_vuelo);

            HttpContext.Session.SetString("correo_cliente_correo", correo_cliente);
            HttpContext.Session.SetString("precio_correo", precio.ToString());


            //Inserta en tabla reservas
            int resultado = 0;
            int resultado2 = 0;
            string vista = "";


            string tip = HttpContext.Session.GetString("tipo_boleto_correo");
            string cant = HttpContext.Session.GetString("cantidad_asientos_correo");
            string clas = HttpContext.Session.GetString("clase_correo");
            string cr = HttpContext.Session.GetString("codigo_reserva_correo");
            string cv = HttpContext.Session.GetString("codigo_vuelo_correo");
            string cc = HttpContext.Session.GetString("correo_cliente_correo");
            string pre = HttpContext.Session.GetString("precio_correo");


            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_insertar_reserva @param_tipo = '{tip}', @param_cantidad = '{cant}',@param_clase = '{clas}',@param_codigo = '{cr}',@param_vuelo = '{cv}',@param_correo = '{cc}',@param_precio = '{pre}'";
                string sql2 = $"EXECUTE sp_insertar_carrito @correo ='{cc}',@codigo ='{cr}',@total ='{pre}'";
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

            if (resultado == 1 && resultado2 == 1)
            {
                vista = "Agregado!";
            }
            else
            {
                vista = "Error";
            }

            //Envia el correo de aviso
            this.SendEmail(codigo_reserva, precio.ToString(), correo_cliente);


            return vista;
        }


        public IActionResult buscarCheckIn()
        {
            return View("BuscarCheckInView");
        }




        public IActionResult cargarCheckIn(string codigo_vuelo_check)
        {
            System.Diagnostics.Debug.WriteLine(codigo_vuelo_check);

            List<int> lista_asientos_ocupados = new List<int>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"EXECUTE sp_obtener_asientos_ocupados @codigo = '{codigo_vuelo_check}'";
                SqlDataReader reader;
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    reader = command.ExecuteReader();
                   
                    while (reader.Read())
                    {
                        
                        lista_asientos_ocupados.Add(Convert.ToInt32(reader[0]));
                        System.Diagnostics.Debug.WriteLine(reader[0]);
                    }
                    connection.Close();

                }
            }

            ViewBag.AsientosOcupados = lista_asientos_ocupados;

            return View("CheckInView");
        }
    }
}