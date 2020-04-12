using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proye3JoseDRodriguezSemestreI2019.Models
{
    public class Vuelo
    {
        public string codigo { get; set; }
        public string lugar_salida { get; set; }
        public string destino { get; set; }
        public string fecha_salida { get; set; }
        public string fecha_llegada { get; set; }
        public string avion { get; set; }
        public string asientos { get; set; }
    }
}
