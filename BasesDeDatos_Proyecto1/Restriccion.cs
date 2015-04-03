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
        //ID
        public String nombre;
        //PK, FK, CH
        public String tipo;
        //Sirve para PK y FK
        public List<String> columnasPropias;
        //Sirve para FK
        public String tabla;
        //Sirve para FK 
        public List<String> columnasForaneas;

        //Expresion del Check
        public String restriccionCheck;

        public Restriccion() {
            nombre = "";
            tipo = "";
            columnasPropias = new List<String>();
            tabla = "";
            columnasForaneas = new List<String>();
        }

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
