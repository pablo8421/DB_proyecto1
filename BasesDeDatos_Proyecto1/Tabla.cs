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
        public List<String> columnas;
        public List<String> tipos_columnas;
        public List<Restriccion> restricciones;

        public Tabla()
        {
            nombre = "";
            cantidad_registros = 0;
            columnas = null;
            tipos_columnas = null;
            restricciones = null;
        }

        public Tabla(String nombre, int cantidad_registros, List<String> columnas, List<String> tipos_columnas, List<Restriccion> restricciones)
        {
            this.nombre = nombre;
            this.cantidad_registros = cantidad_registros;
            this.columnas = columnas;
            this.tipos_columnas = tipos_columnas;
            this.restricciones = restricciones;
        }

        public void generarColumnas(List<String> columnas)
        {
            this.columnas = new List<String>();
            tipos_columnas= new List<String>();
            foreach (String columna in columnas)
            {
                String[] tupla = columna.Split(' ');
                tipos_columnas.Add(tupla[0]);
                this.columnas.Add(tupla[1]);
            }
        }
    }
}
