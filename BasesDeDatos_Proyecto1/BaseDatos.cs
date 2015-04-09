/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que modela una abse de datos
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class BaseDatos
    {
        public String nombre { set; get; }          //Nombre de la base de datos
        public int cantidad_tablas { set; get; }    //Cantidad de tablas que tiene la base de datos
        public int registros { set; get; }          //Cantidad de registros que tiene la base de datos

        //Constructor
        public BaseDatos()
        {
            nombre = "";
            cantidad_tablas = 0;
            registros = 0;
        }

        //Constructor con nombre personalizado
        public BaseDatos(String nombre) {
            this.nombre = nombre;
            cantidad_tablas = 0;
            registros = 0;
        }
    }
}
