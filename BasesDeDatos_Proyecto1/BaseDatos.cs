using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class BaseDatos
    {
        public String nombre { set; get; }
        public int cantidad_tablas { set; get; }

        public BaseDatos()
        {
            nombre = "";
            cantidad_tablas = 0;
        }

        public BaseDatos(String nombre) {
            this.nombre = nombre;
            cantidad_tablas = 0;
        }
    }
}
