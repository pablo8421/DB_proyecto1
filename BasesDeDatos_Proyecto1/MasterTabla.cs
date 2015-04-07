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
        public List<Tabla> tablas;

        public MasterTabla() {
            tablas = new List<Tabla>();
        }

        public void agregarTabla(Tabla t)
        {
            tablas.Add(t);
        }

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
