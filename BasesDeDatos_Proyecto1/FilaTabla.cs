﻿/*
Pablo Sánchez, 12148
César Guerra, 12593
Sección 10
Clase que maneja una tabla y los registros que esta contiene.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsgPack.Serialization;
using System.IO;

namespace BasesDeDatos_Proyecto1
{
    class FilaTabla
    {
        public Tabla tabla;     //Variable que modela la tabla de la BD
        String BDenUso;         //Variable que contiene la información de la BD en uso.
        public Filas datos;     //Variable que contiene los datos de las tablas

        //Constructor de la clase
        public FilaTabla(Tabla tabla, String BDenUso)
        {
            this.tabla= tabla;
            this.BDenUso = BDenUso;
            datos = new Filas();
        }

        //Almacena los registros de la tabla
        public void guardar()
        {
            String path = "Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat";
            System.IO.File.Delete(path);

            //Crear el archivo vacio de la tabla
            path = System.IO.Path.Combine(Path.GetFullPath("Databases"), BDenUso);

            string fileName = tabla.nombre + ".dat";
            path = System.IO.Path.Combine(path, fileName);
            System.IO.FileStream fs = System.IO.File.Create(path);
            fs.Close();

            // Creates serializer.
            var serializer = SerializationContext.Default.GetSerializer<Filas>();
            // Pack obj to stream.
            using (Stream stream = File.Open("Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat", FileMode.Open))
            {
                serializer.Pack(stream, datos);
                stream.Close();
            }
        }

        //Obtiene los registro de la tabla
        public void cargar()
        {
            try
            {
                var serializer = MessagePackSerializer.Get<Filas>();
                using (Stream stream = File.Open("Databases\\" + BDenUso + "\\" + tabla.nombre + ".dat", FileMode.Open))
                {
                    datos = serializer.Unpack(stream);
                    stream.Close();
                }

            }
            catch (Exception e)
            {
                datos = new Filas();
            }
        }

        //Devuelve la cantidad de registros que tiene la tabla
        public int getTamanio()
        {
            return tabla.cantidad_registros;
        }

        //Devuelve un registro de la tabla
        public List<Object> getRow(int num)
        {
            return datos.elementos[num];
        }

        //Devuelve un registro de la tabla (Realiza casting a los datos)
        public object[] obtenerFila(int num)
        {
            object[] resultado = new object[datos.elementos[num].Count];
            for (int i = 0; i < resultado.Length; i++ )
            {
                if (datos.elementos[num][i] is MsgPack.MessagePackObject)
                    if (!((MsgPack.MessagePackObject)datos.elementos[num][i]).IsNil)
                        if (tabla.tipos_columnas[i].Equals("INT"))
                        {
                            resultado[i] = ((MsgPack.MessagePackObject)datos.elementos[num][i]).AsInt32();
                        }
                        else if (tabla.tipos_columnas[i].Equals("FLOAT"))
                        {
                            resultado[i] = ((MsgPack.MessagePackObject)datos.elementos[num][i]).AsSingle();
                        }
                        else if (tabla.tipos_columnas[i].Equals("DATE"))
                        {
                            resultado[i] = ((MsgPack.MessagePackObject)datos.elementos[num][i]).AsString();
                        }
                        else if (tabla.tipos_columnas[i].StartsWith("CHAR"))
                        {
                            resultado[i] = ((MsgPack.MessagePackObject)datos.elementos[num][i]).AsString();
                        }
                        else
                        {
                            return null;
                        }
                    else
                        resultado[i] = null;
                else
                    resultado[i] = datos.elementos[num][i];
            }
            return resultado;
        }

        //Devuelve una celda de una fila de la tabla
        public Object getRowElement(int row, int columna)
        {
            if (!((MsgPack.MessagePackObject)datos.elementos[row][columna]).IsNil)
                if (tabla.tipos_columnas[columna].Equals("INT"))
                {
                    return  ((MsgPack.MessagePackObject) datos.elementos[row][columna]).AsInt32();
                }
                else if (tabla.tipos_columnas[columna].Equals("FLOAT"))
                {
                    return ((MsgPack.MessagePackObject)datos.elementos[row][columna]).AsSingle();
                }
                else if (tabla.tipos_columnas[columna].Equals("DATE"))
                {
                    return ((MsgPack.MessagePackObject)datos.elementos[row][columna]).AsString();
                }
                else if (tabla.tipos_columnas[columna].StartsWith("CHAR"))
                {
                    return ((MsgPack.MessagePackObject)datos.elementos[row][columna]).AsString();
                }
                else
                {
                    return null;
                }
            else
                return null;
        }

        //Agrega un nuevo registro a la tabla
        public void agregarFila(List<Object> fila)
        {
            tabla.cantidad_registros++;
            datos.elementos.Add(fila);
        }        
    }

    //Clase que modela la tabla que almacena los datos
    public class Filas
    {
        public List<List<Object>> elementos { get; set; }

        //Constructor
        public Filas()
        {
            elementos = new List<List<Object>>();
        }
    }
}
