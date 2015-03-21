using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    class BaseDatos
    {
        private String nombre { set; get; }
        private int cantidad_tablas { set; get; }

        public BaseDatos(String nombre) {
            this.nombre = nombre;
            cantidad_tablas = 0;
        }
    }
}
