using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasesDeDatos_Proyecto1
{
    public class MasterTabla
    {
        List<Tabla> tablas;

        public MasterTabla() {
            tablas = new List<Tabla>();
        }

        public void agregarBD(Tabla t)
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
