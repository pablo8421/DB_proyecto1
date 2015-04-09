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
        public List<BaseDatos> basesDeDatos;    //Variable que contiene la lista de bases de datos

        //Constructor
        public MasterBD() { 
            basesDeDatos = new List<BaseDatos>();
        }

        //Agrega una base de datos
        public void agregarBD(BaseDatos bd){
            basesDeDatos.Add(bd);
        }

        //Verifica si contiene una base de datos que tenga el mismo nombre que el parámetro enviado
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

        //Elimina una base de datos
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

        //Obtiene la cantidad de registros que contiene una base de datos
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

        //Actualiza la cantidad de tablas en la base de datos
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

        //Obtiene la base de datos pedida
        public BaseDatos getBD(String nombre){
            foreach (BaseDatos bd in basesDeDatos)
                if (nombre.Equals(bd.nombre))
                    return bd;
            return null;
        }
    }
}
