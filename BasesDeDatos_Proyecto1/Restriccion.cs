/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase encargada de manejar las restricciones de las columnas.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BasesDeDatos_Proyecto1
{
    public class Restriccion
    {

        public String nombre;                   //Variable que guarda el nombre de la restricción
        public String tipo;                     //Variable que guarda el tipo de restricción  (PK, FK, CHECK)
        public List<String> columnasPropias;    //Variable que guarda las columnas a las que hace referencia una restricción dentro de una tabla (PK, FK)
        public String tabla;                    //Variable que guarda el nombre de la tabla a la que hace referencia una restricción (FK)
        public List<String> columnasForaneas;   //Variable que guarda los nombres de las columnas que son referenciadas y pertenecen a la tabla mencionada en la variable anterior (FK)
        public String restriccionCheck;         //Variable que guarda en postfix la restricción que se colocó en el check

        //Constructor
        public Restriccion() {
            nombre = "";
            tipo = "";
            columnasPropias = new List<String>();
            tabla = "";
            columnasForaneas = new List<String>();
        }

        //Constructor con tipo personalizado
        public Restriccion(String tipo)
        {
            nombre = "";
            this.tipo = tipo;
            if (tipo.Equals("PK"))
            {
                columnasPropias = new List<String>();
            }
            else if (tipo.Equals("FK"))
            {
                columnasPropias = new List<String>();
                tabla = "";
                columnasForaneas = new List<String>();

            }
            else if (tipo.Equals("CH"))
            {
                restriccionCheck = "";
            }

        }
        
        //Realiza la descripción de las restricciones
        override
        public string ToString(){
            string descripcion = "\"";
            if (!nombre.Equals(""))
            {
                descripcion += "CONSTRAINT "+nombre+" ";
                if (tipo.Equals("PK"))
                    descripcion += "PRIMARY KEY";
                if (tipo.Equals("FK"))
                    descripcion += "FOREIGN KEY";
                if (tipo.Equals("CH"))
                    descripcion += "CHECK("+restriccionCheck+")";
                if (tabla!=null)
                    if (!tabla.Equals(""))
                        descripcion += " REFERENCES " + tabla;
                    else
                        descripcion += "\"";
                else
                    descripcion += "\"";
            }
            return descripcion;
        }
        
    }
}
