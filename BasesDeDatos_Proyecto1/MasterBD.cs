using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BasesDeDatos_Proyecto1
{
    public class MasterBD
    {
        public List<BaseDatos> basesDeDatos;

        public MasterBD() { 
            basesDeDatos = new List<BaseDatos>();
        }

        public void agregarBD(BaseDatos bd){
            basesDeDatos.Add(bd);
        }

        public bool containsBD(String nombre)
        {
            foreach (BaseDatos bd in basesDeDatos)
            {
                if (bd.nombre.Equals(nombre))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
