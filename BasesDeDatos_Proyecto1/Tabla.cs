/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que modela una tabla
*/
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
            columnas = new List<String>();
            tipos_columnas = new List<String>();
            restricciones = new List<Restriccion>();
        }

        public Tabla(String nombre, int cantidad_registros, List<String> columnas, List<String> tipos_columnas, List<Restriccion> restricciones)
        {
            this.nombre = nombre;
            this.cantidad_registros = cantidad_registros;
            this.columnas = columnas;
            this.tipos_columnas = tipos_columnas;
            this.restricciones = restricciones;
        }

        public bool generarColumnas(List<String> columnas)
        {
            this.columnas = new List<String>();
            tipos_columnas= new List<String>();
            foreach (String columna in columnas)
            {
                String[] tupla = columna.Split(' ');
                if (this.columnas.Contains(tupla[0]))
                {
                    this.columnas[0] = tupla[0];
                    return false;
                }
                tipos_columnas.Add(tupla[1]);
                this.columnas.Add(tupla[0]);
            }
            return true;
        }
    }
}
