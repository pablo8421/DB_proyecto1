using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class Restriccion
    {
        //ID
        public String nombre;
        //PK, FK, CH
        public String tipo;
        //Sirve para PK y FK
        public List<int> columnasPropias;
        //Sirve para FK
        public String tabla;
        //Sirve para FK 
        public List<int> columnasForaneas;

        //Expresion del Check
        public String restriccionCheck;

        public Restriccion() {
            nombre = "";
            tipo = "";
            columnasPropias = new List<int>();
            tabla = "";
            columnasForaneas = new List<int>();
        }

        public Restriccion(String tipo)
        {
            nombre = "";
            this.tipo = tipo;
            if (tipo.Equals("PK"))
            {
                columnasPropias = new List<int>();
            }
            else if (tipo.Equals("FK"))
            {
                columnasPropias = new List<int>();
                tabla = "";
                columnasForaneas = new List<int>();

            }
            else if (tipo.Equals("CH"))
            {
                restriccionCheck = "";
            }

        }
        
        //FALTA
        override
        public string ToString(){
            string descripcion = "";
            if (!nombre.Equals(""))
            {
                descripcion += nombre;
                if (!tipo.Equals(""))
                {
                    descripcion += ", " + tipo;
                    if (!tabla.Equals(""))
                    {
                        descripcion += ", REFERENCES " + tabla;
                        if (columnasForaneas.Count != 0)
                        {
                            descripcion += columnasForaneas.ToString();
                        }
                    }
                }
            }
            return descripcion;
        }
        //COMENTARIO
    }
}
