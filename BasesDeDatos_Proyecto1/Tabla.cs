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
        public String nombre;                       //Variable que almacena el nombre de la tabla
        public int cantidad_registros;              //Variable que almacena la cantidad de registros que contiene la tabla
        public List<String> columnas;               //Variable que almacena las columnas de la tabla
        public List<String> tipos_columnas;         //Variable que almacena los tipos de las columnas de la tabla
        public List<Restriccion> restricciones;     //Variable que almacena las restricciones de la tabla

        //Constructor
        public Tabla()
        {
            nombre = "";
            cantidad_registros = 0;
            columnas = new List<String>();
            tipos_columnas = new List<String>();
            restricciones = new List<Restriccion>();
        }

        //Constructor personalizado
        public Tabla(String nombre, int cantidad_registros, List<String> columnas, List<String> tipos_columnas, List<Restriccion> restricciones)
        {
            this.nombre = nombre;
            this.cantidad_registros = cantidad_registros;
            this.columnas = columnas;
            this.tipos_columnas = tipos_columnas;
            this.restricciones = restricciones;
        }

        //Genera las columnas de una tabla en base a una lista
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
