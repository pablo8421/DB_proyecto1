/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que modela una coleccion de tablas
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class MasterTabla
    {
        public List<Tabla> tablas;  //Variable que almacena la lista de tablas de una base de datos

        //Constructor
        public MasterTabla() {
            tablas = new List<Tabla>();
        }

        //Agrega una tabla a una base de datos
        public void agregarTabla(Tabla t)
        {
            tablas.Add(t);
        }

        //Verifica si contiene una tabla con el nombre que se envió en el parametro
        public bool containsTable(String nombre)
        {
            foreach (Tabla t in tablas)
            {
                if (t.nombre.Equals(nombre))
                {
                    return true;
                }
            }
            return false;
        }

        //Obtiene una tabla de la base de datos
        public Tabla getTable(String nombre)
        {
            foreach (Tabla t in tablas)
            {
                if (t.nombre.Equals(nombre))
                {
                    return t;
                }
            }
            return null;
        }

        //Elimina una tabla de la base de datos
        public void borrarTabla(String nombre)
        {
            foreach (Tabla t in tablas)
            {
                if (t.nombre.Equals(nombre))
                {
                    tablas.Remove(t);
                    break;
                }
            }
        }
    }
}
