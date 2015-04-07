/*
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
        public Tabla tabla;
        String BDenUso;
        public Filas datos;

        public FilaTabla(Tabla tabla, String BDenUso)
        {
            this.tabla= tabla;
            this.BDenUso = BDenUso;
            datos = new Filas();
        }

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

        public int getTamanio()
        {
            return tabla.cantidad_registros;
        }


        public List<Object> getRow(int num)
        {
            return datos.elementos[num];
        }

        public object[] obtenerFila(int num)
        {
            object[] resultado = new object[datos.elementos[num].Count];
            for (int i = 0; i < resultado.Length; i++ )
            {
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
            }
            return resultado;
        }

        public Object getRowElement(int row, int columna)
        {
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
        }

        public void agregarFila(List<Object> fila)
        {
            tabla.cantidad_registros++;
            datos.elementos.Add(fila);
        }


        public void mostrarTablaEnConsola()
        {
            for (int i = 0; i < tabla.cantidad_registros; i++)
            {
                int j = 0;
                foreach (MsgPack.MessagePackObject item in datos.elementos[i])
                {
                    if (tabla.tipos_columnas[j].Equals("INT"))
                    {
                        Console.Write(item.AsInt32() + "  ");
                    }
                    else if (tabla.tipos_columnas[j].Equals("FLOAT"))
                    {
                        Console.Write(item.AsSingle() + "  ");
                    }
                    else if (tabla.tipos_columnas[j].Equals("DATE"))
                    {
                        Console.Write(item.AsString() + "  ");
                    }
                    else if (tabla.tipos_columnas[j].StartsWith("CHAR"))
                    {
                        Console.Write(item.AsString() + "  ");
                    }
                    else
                    {
                        Console.Write("NOPE");
                    }
                    j++;
                }
                Console.WriteLine();
            }
        }

    }

    public class Filas
    {
        public List<List<Object>> elementos { get; set; }

        public Filas()
        {
            elementos = new List<List<Object>>();
        }
    }
}
