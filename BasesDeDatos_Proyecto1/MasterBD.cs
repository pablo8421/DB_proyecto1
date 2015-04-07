/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que maneja una coleccion de bases de datos
*/
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

        public void borrarBD(String nombre)
        {
            foreach (BaseDatos bd in basesDeDatos)
            {
                if (bd.nombre.Equals(nombre))
                {
                    basesDeDatos.Remove(bd);
                    break;
                }
            }
        }

        public int getRegistros(String nombre){
            foreach (BaseDatos bd in basesDeDatos)
            {
                if (bd.nombre.Equals(nombre))
                {
                    return bd.registros;
                }
            }
            return 0;
        }

        public void actualizarCantidadEnBD(string nombre, int cantidad)
        {
            foreach (BaseDatos bd in basesDeDatos)
            {
                if (bd.nombre.Equals(nombre))
                {
                    bd.cantidad_tablas = cantidad;
                }
            }
        }

        public BaseDatos getBD(String nombre){
            foreach (BaseDatos bd in basesDeDatos)
                if (nombre.Equals(bd.nombre))
                    return bd;
            return null;
        }
    }
}
