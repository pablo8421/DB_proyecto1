using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class Tabla 
    {
        public String nombre;
        public int cantidad_registros;
        public String[] columnas;
        public String[] tipos_columnas;
        public String[] restricciones;

        public Tabla()
        {
            nombre = "";
            cantidad_registros = 0;
            columnas = null;
            tipos_columnas = null;
            restricciones = null;
        }

        public Tabla(String nombre, int cantidad_registros, String[] columnas, String[] tipos_columnas, String[] restricciones) {
            this.nombre = nombre;
            this.cantidad_registros = cantidad_registros;
            this.columnas = columnas;
            this.tipos_columnas = tipos_columnas;
            this.restricciones = restricciones;
        }
    }
}
