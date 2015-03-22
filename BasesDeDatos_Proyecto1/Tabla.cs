using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    class Tabla 
    {
        private String nombre;
        private int cantidad_registros;
        private String[] columnas;
        private String[] tipos_columnas;
        private String[] restricciones;

        public Tabla(String nombre, int cantidad_registros, String[] columnas, String[] tipos_columnas, String[] restricciones) {
            this.nombre = nombre;
            this.cantidad_registros = cantidad_registros;
            this.columnas = columnas;
            this.tipos_columnas = tipos_columnas;
            this.restricciones = restricciones;
        }
    }
}
