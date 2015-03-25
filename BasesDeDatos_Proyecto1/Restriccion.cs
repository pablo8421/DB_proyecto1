using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class Restriccion
    {
        public String nombre;
        public String tipo;
        public List<int> columnas;
        public String tabla;
        public List<int> columnasTabla;

        public Restriccion() {
            nombre = "";
            tipo = "";
            columnas = new List<int>();
            tabla = "";
            columnasTabla = new List<int>();
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
                    if (columnas.Count != 0)
                        descripcion += ", " + columnas.ToString();
                }
                else
                    if (columnas.Count != 0)
                        descripcion += ", " + columnas.ToString();
            }
            else {
                if (!tipo.Equals(""))
                {
                    descripcion += tipo;
                    if (columnas.Count != 0)
                        descripcion += ", " + columnas.ToString();
                }
                else
                    if (columnas.Count != 0)
                        descripcion += columnas.ToString();
            }
            return descripcion;
        }
    }
}
