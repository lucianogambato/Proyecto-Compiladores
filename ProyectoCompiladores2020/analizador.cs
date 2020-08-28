﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime;

namespace ProyectoCompiladores2020
{
    class analizador
    {
        public analizador()
        {
            simbolos();
        }
        private Dictionary<int, string> archivo = new Dictionary<int, string>();
        public void guardarArchivo(Dictionary<int, string> lineas)
        {
            archivo = lineas;
        }
        public bool correcto = true;
        List<string> simbolosOtros = new List<string>();
        public void simbolos()
        {
            simbolosOtros.Add("+");
            simbolosOtros.Add("-");
            simbolosOtros.Add("*");
            simbolosOtros.Add("/");
            simbolosOtros.Add("%");
            simbolosOtros.Add("<");
            simbolosOtros.Add("<=");
            simbolosOtros.Add(">");
            simbolosOtros.Add(">=");
            simbolosOtros.Add("=");
            simbolosOtros.Add("==");
            simbolosOtros.Add("!=");
            simbolosOtros.Add("&&");
            simbolosOtros.Add("||");
            simbolosOtros.Add("!");
            simbolosOtros.Add(";");
            simbolosOtros.Add(",");
            simbolosOtros.Add(".");
            simbolosOtros.Add("(");
            simbolosOtros.Add(")");
            simbolosOtros.Add("()");
            simbolosOtros.Add("[");
            simbolosOtros.Add("]");
            simbolosOtros.Add("[]");
            simbolosOtros.Add("{");
            simbolosOtros.Add("}");
            simbolosOtros.Add("{}");
        }
        public List<string> errores = new List<string>();
        
        Regex id = new Regex("^([a-z|A-Z]+([_]|[0-9]|[a-z|A-Z])*)$");
        Regex enterosB10 = new Regex("^[0-9]+$");
        Regex decimales = new Regex("^[0-9]+[.]([0-9]*)([eE][+-]?)?[0-9]*$");
        Regex enterosB16 = new Regex("^([0][xX])([0-9]|[abcdefABCDEF])*$");
        Regex booleano = new Regex("true|false");
        Regex reservadas = new Regex(@"^(void|int|double|bool|string|class|const|interface|null|this|for|while|foreach|if|else|return|break|New|NewArray|Console|WriteLine)$");
        Regex caracterNoValido = new Regex("^[a-z|A-Z|0-9|&|_]+$");
        bool comment = false;
        public List<string> Reconocedor()
        {

            int llave = 1;
            string concatpalabra = String.Empty;
            var salida = new List<string>();
            do
            {
                var linea = new Queue<char>();
                foreach (var item in archivo[llave].ToCharArray())
                {
                    linea.Enqueue(item);
                }
                int inicio = 1;
                int fin = 1;
                string cadena = "";
                string inicioComments = "";
                int tamanio = linea.Count;
                for (int i = linea.Count; i > 0; i++)
                {
                    if (fin > tamanio)
                    {
                        fin--;
                    }
                    if (linea.Count == 0 && cadena == "")
                    {
                        break;
                    }
                    else if (comment == true)
                    {
                        inicioComments += linea.Dequeue().ToString();
                        while (comment == true)
                        {
                            if (inicioComments.Contains("*/"))
                            {
                                comment = false;
                                break;
                            }
                            else if (linea.Count > 0)
                            {
                                inicioComments += linea.Dequeue().ToString();
                            }
                            else if (linea.Count == 0 && archivo.Count == llave)
                            {
                                salida.Add("*** EOF en Comentario");
                                errores.Add("*** EOF en Comentario");
                                correcto = false;
                                break;
                            }
                            else if (linea.Count == 0)
                            {
                                inicioComments += "\n";
                                llave++;
                                foreach (var item in archivo[llave].ToCharArray())
                                {
                                    linea.Enqueue(item);
                                }

                            }

                        }
                        inicioComments = "";
                        comment = false;
                    }
                    else if (linea.Count == 0 && cadena != "")
                    {
                        if (fin < tamanio)
                            fin--;
                        else if (fin == tamanio && linea.Count != 0)
                        { fin--; }
                        ////////////////////////////////////////
                        if (validarCadena(cadena, llave, inicio, fin) == "")
                        {
                            string dividir = cadena;
                            var error = dividir.ToCharArray();
                            int finError = inicio;
                            string palabras = "";
                            var colaPalabraDivirdir = new Queue<string>();
                            foreach (var item in error)
                            {
                                colaPalabraDivirdir.Enqueue(item.ToString());
                            }
                            do
                            {

                                if (validarCadena(palabras + colaPalabraDivirdir.Peek().ToString(), llave, inicio, fin) != "")
                                {
                                    palabras += colaPalabraDivirdir.Dequeue().ToString();
                                    finError++;
                                }
                                else
                                {
                                    if (finError < fin)
                                        finError--;
                                    else if (fin == finError)
                                    { finError--; }
                                    salida.Add(validarCadena(palabras, llave, inicio, finError));
                                    finError++;
                                    inicio = finError;
                                    palabras = "";
                                }
                            } while (colaPalabraDivirdir.Count != 0);
                            if (palabras != "")
                            {
                                if (finError > fin)
                                    finError--;
                                else if (fin == finError)
                                { finError--; }
                                salida.Add(validarCadena(palabras, llave, inicio, finError));
                                finError++;
                                inicio = finError;
                                fin = inicio;
                                palabras = "";
                            }
                        }
                        else
                        {
                            salida.Add(validarCadena(cadena, llave, inicio, fin));
                            fin = inicio;
                        }
                        cadena = "";
                        if (linea.Count != 0)
                        {
                            if (linea.Peek() == ' ' || linea.Peek() == '\t')
                            {
                                linea.Dequeue();
                                inicio += 2;
                                fin = inicio;
                            }
                            else
                            {
                                inicio ++;
                                fin = inicio;
                            }
                        }
                    }
                    else
                    {
                        if (cadena == "_")
                        {
                            salida.Add("*** Error en linea " + llave + "caracter no reconocido: " + "'" + cadena + "'");
                            errores.Add("*** Error en linea " + llave + "caracter no reconocido: " + "'" + cadena + "'");
                            correcto = false;
                            inicio = fin;
                            cadena = "";

                        }
                        else if (linea.Peek() == '/')
                        {
                            if (cadena != "")
                            {
                                if (fin < tamanio)
                                    fin--;
                                else if (fin == tamanio && linea.Count != 0)
                                { fin--; }
                                /////////////////////////////////////////////////////
                                if (validarCadena(cadena, llave, inicio, fin) == "")
                                {
                                    string dividir = cadena;
                                    var error = dividir.ToCharArray();
                                    int finError = inicio;
                                    string palabras = "";
                                    var colaPalabraDivirdir = new Queue<string>();
                                    foreach (var item in error)
                                    {
                                        colaPalabraDivirdir.Enqueue(item.ToString());
                                    }
                                    do
                                    {

                                        if (validarCadena(palabras + colaPalabraDivirdir.Peek().ToString(), llave, inicio, fin) != "")
                                        {
                                            palabras += colaPalabraDivirdir.Dequeue().ToString();
                                            finError++;
                                        }
                                        else
                                        {
                                            if (finError < fin)
                                                finError--;
                                            else if (fin == finError)
                                            { finError--; }
                                            salida.Add(validarCadena(palabras, llave, inicio, finError));
                                            finError++;
                                            inicio = finError;
                                            palabras = "";
                                        }
                                    } while (colaPalabraDivirdir.Count != 0);
                                    if (palabras != "")
                                    {
                                        if (finError > fin)
                                            finError--;
                                        else if (fin == finError)
                                        { finError--; }
                                        salida.Add(validarCadena(palabras, llave, inicio, finError));
                                        finError++;
                                        inicio = finError;
                                        fin = inicio;
                                        palabras = "";
                                    }
                                }
                                else
                                {
                                    salida.Add(validarCadena(cadena, llave, inicio, fin));
                                    fin++;
                                    inicio = fin;
                                    //fin = inicio;
                                }
                                //fin++;
                                //inicio = fin;
                                cadena = "";

                            }
                            inicioComments += linea.Dequeue().ToString();
                            if(linea.Count != 0)
                            { 
                                if (linea.Peek() == '/')
                                {
                                    do
                                    {
                                        inicioComments += linea.Dequeue().ToString();
                                        fin++;
                                    } while (linea.Count > 0);
                                    inicio = fin;
                                }
                                else if (linea.Peek() == '*')
                                {
                                    inicioComments += linea.Dequeue().ToString();
                                    comment = true;
                                }
                                else
                                {
                                    if (cadena != "")
                                    {
                                        salida.Add(validarCadena(cadena, llave, inicio, fin));
                                        cadena = "";
                                        inicio = fin;
                                    }
                                    salida.Add(inicioComments + " en linea " + llave.ToString() + " cols " + inicio + " - " + fin + " es T_Operador");

                                    fin++;
                                    inicio = fin;
                                }
                            }
                            else
                            {
                                salida.Add(inicioComments +" en linea "+ llave +"cols "+inicio+" - "+fin+ "es T_operador");
                            }
                        }
                        else if (linea.Peek() == '*')
                        {
                            linea.Dequeue();
                            fin++;
                            inicio++;
                            if (linea.Count > 0)
                            {
                                if (linea.Peek() == '/')
                                {
                                    linea.Dequeue();
                                    fin++;
                                    inicio++;
                                    salida.Add("***Salida de comentario sin emparejar" + " en linea " + llave.ToString());
                                    errores.Add("***Salida de comentario sin emparejar" + " en linea " + llave.ToString());
                                    correcto = false;
                                }
                                else
                                {
                                    if (fin < tamanio)
                                        fin--;
                                    else if (fin == tamanio && linea.Count != 0)
                                    { fin--; }
                                    inicio = fin;
                                    salida.Add("simbolo : " + "*" + " en linea " + llave.ToString() + " cols " + inicio + " - " + fin);
                                    fin++;
                                    inicio = fin;
                                }
                            }
                        }
                        else if (linea.Peek() == '"')
                        {
                            bool correcto = true;
                            do
                            {
                                cadena += linea.Dequeue().ToString();
                                fin++;
                                if (linea.Count == 0)
                                {
                                    salida.Add("Constante string sin terminar" + llave); 
                                    errores.Add("Constante string sin terminar" + llave);
                                    correcto = false;

                                    inicio = fin;
                                    break;
                                    correcto = false;
                                }

                            } while (linea.Peek() != '"');
                            if (correcto && linea.Count > 0)
                            {
                                cadena += linea.Dequeue().ToString();
                                
                                salida.Add( cadena + " en linea " + llave + " cols " + inicio + " - " + fin + " es T_ConstanteString" + " (valor = " +cadena+")");
                                fin++;
                                inicio = fin;
                                cadena = "";
                            }
                            cadena = "";
                        }
                        else if (enterosB10.IsMatch(cadena) && linea.Peek() == '.')
                        {
                            cadena += linea.Dequeue().ToString();
                            if (linea.Count == 0)
                            {
                                //if (fin < tamanio)
                                //    fin--;
                                salida.Add(validarCadena(cadena, llave, inicio, fin));
                                inicio = fin;
                                cadena = "";
                            }
                            else
                            {
                                while (linea.Peek() != ' ' && linea.Peek() != '.' && decimales.IsMatch(cadena + linea.Peek().ToString()))
                                {
                                    cadena += linea.Dequeue().ToString();
                                    fin++;
                                    if (linea.Count == 0)
                                    {
                                        break;
                                    }

                                }
                                /////////////////////////////////////////
                                if (validarCadena(cadena, llave, inicio, fin) == "")
                                {
                                    string dividir = cadena;
                                    var error = dividir.ToCharArray();
                                    int finError = inicio;
                                    string palabras = "";
                                    var colaPalabraDivirdir = new Queue<string>();
                                    foreach (var item in error)
                                    {
                                        colaPalabraDivirdir.Enqueue(item.ToString());
                                    }
                                    do
                                    {

                                        if (validarCadena(palabras + colaPalabraDivirdir.Peek().ToString(), llave, inicio, fin) != "")
                                        {
                                            palabras += colaPalabraDivirdir.Dequeue().ToString();
                                            finError++;
                                        }
                                        else
                                        {
                                            if (finError < fin)
                                                finError--;
                                            else if (fin == finError)
                                            { finError--; }
                                            salida.Add(validarCadena(palabras, llave, inicio, finError));
                                            finError++;
                                            inicio = finError;
                                            palabras = "";
                                        }
                                    } while (colaPalabraDivirdir.Count != 0);
                                    if (palabras != "")
                                    {
                                        if (finError > fin)
                                            finError--;
                                        else if (fin == finError)
                                        { finError--; }
                                        salida.Add(validarCadena(palabras, llave, inicio, finError));
                                        finError++;
                                        inicio = finError;
                                        fin = inicio;
                                        palabras = "";
                                    }
                                }
                                else
                                {
                                    salida.Add(validarCadena(cadena, llave, inicio, fin));
                                    //fin++;
                                    inicio = fin;
                                }
                                //inicio = fin;
                                cadena = "";
                                if (linea.Count != 0)
                                {
                                    inicio++;
                                    fin = inicio;
                                }


                            }
                        }
                        else if (linea.Peek() != '\n' && linea.Peek() != '\t' && linea.Peek() != ' ' && !simbolosOtros.Contains(linea.Peek().ToString()) && (caracterNoValido.IsMatch(linea.Peek().ToString()) || (linea.Peek() == '|' )) && cadena.Length < 31)
                        {
                            cadena += linea.Dequeue().ToString();
                            fin++;
                        }
                        else if (simbolosOtros.Contains(linea.Peek().ToString()) || linea.Peek() == '+' || linea.Peek() == '-' || linea.Peek() == '.')
                        {
                            if (cadena != "")
                            {
                                if (fin < tamanio)
                                    fin--;
                                else if( fin==tamanio && linea.Count != 0)
                                { fin--; }
                                ///////////////////////////////////////
                                if (validarCadena(cadena, llave, inicio, fin) == "")
                                {
                                    string dividir = cadena;
                                    var error = dividir.ToCharArray();
                                    int finError = inicio;
                                    string palabras = "";
                                    var colaPalabraDivirdir = new Queue<string>();
                                    foreach (var item in error)
                                    {
                                        colaPalabraDivirdir.Enqueue(item.ToString());
                                    }
                                    do
                                    {

                                        if (validarCadena(palabras + colaPalabraDivirdir.Peek().ToString(), llave, inicio, fin) != "")
                                        {
                                            palabras += colaPalabraDivirdir.Dequeue().ToString();
                                            finError++;
                                        }
                                        else
                                        {
                                            if (finError < fin)
                                                finError--;
                                            else if (fin == finError)
                                            { finError--; }
                                            salida.Add(validarCadena(palabras, llave, inicio, finError));
                                            finError++;
                                            inicio = finError;
                                            palabras = "";
                                        }
                                    } while (colaPalabraDivirdir.Count != 0);
                                    if (palabras != "")
                                    {
                                        if (finError > fin)
                                            finError--;
                                        else if (fin == finError)
                                        { finError--; }
                                        salida.Add(validarCadena(palabras, llave, inicio, finError));
                                        finError++;
                                        inicio = finError;
                                        fin = inicio;
                                        palabras = "";
                                    }
                                }
                                else
                                {
                                    salida.Add(validarCadena(cadena, llave, inicio, fin));
                                    fin++;
                                    inicio = fin;
                                    //fin = inicio;
                                }
                                //fin++;
                                //inicio = fin;
                                cadena = "";

                            }
                            string cadenaSimbolos = "";
                            cadenaSimbolos = linea.Dequeue().ToString();
                            if (linea.Count == 0)
                            {
                                salida.Add(cadenaSimbolos + " en linea " + llave + " cols " + inicio + " - " + fin + " es T_Operador");
                                inicio = fin;
                            }
                            else if (simbolosOtros.Contains(cadenaSimbolos + linea.Peek().ToString()) && cadenaSimbolos != "")
                            {
                                cadenaSimbolos += linea.Dequeue().ToString();
                                fin++;
                                salida.Add(cadenaSimbolos + " en linea " + llave.ToString() + " cols " + inicio + " - " + (fin).ToString() + " es T_Operador");
                                inicio = fin;
                            }
                            else
                            {
                                //fin++;
                                //inicio = fin;
                                salida.Add(cadenaSimbolos + " en linea " + llave.ToString() + " cols " + (inicio) + " - " + fin +" es T_Operador");
                                //fin++;
                                //inicio = fin;
                            }
                            if (linea.Count != 0)
                            {
                                if (linea.Peek() == ' ' || linea.Peek() == '\t')
                                {
                                    inicio += 2;
                                    fin = inicio;
                                    linea.Dequeue();
                                }
                                else
                                {
                                    inicio++;
                                    fin = inicio;
                                }
                            }
                        }
                        else if (cadena != "")
                        {
                            if (fin < tamanio)
                                fin--;
                            else if (fin == tamanio && linea.Count != 0)
                            { fin--; }
                            //////////////////////////////////////
                            if (validarCadena(cadena, llave, inicio, fin) == "")
                            {
                                string dividir = cadena;
                                var error = dividir.ToCharArray();
                                int finError = inicio;
                                string palabras = "";
                                var colaPalabraDivirdir = new Queue<string>();
                                foreach (var item in error)
                                {
                                    colaPalabraDivirdir.Enqueue(item.ToString());
                                }
                                do
                                {

                                    if (validarCadena(palabras + colaPalabraDivirdir.Peek().ToString(), llave, inicio, fin) != "")
                                    {
                                        palabras += colaPalabraDivirdir.Dequeue().ToString();
                                        finError++;
                                    }
                                    else
                                    {
                                        if (finError < fin)
                                            finError--;
                                        else if (fin == finError)
                                        { finError--; }
                                        salida.Add(validarCadena(palabras, llave, inicio, finError));
                                        finError++;
                                        inicio = finError;
                                        palabras = "";
                                    }
                                } while (colaPalabraDivirdir.Count != 0);
                                if (palabras != "")
                                {
                                    if (finError > fin)
                                        finError--;
                                    else if (fin == finError)
                                    { finError--; }
                                    salida.Add(validarCadena(palabras, llave, inicio, finError));
                                    finError++;
                                    inicio = finError;
                                    fin = inicio;
                                    palabras = "";
                                }
                            }
                            else
                            {
                                salida.Add(validarCadena(cadena, llave, inicio, fin));
                                //fin++;
                                inicio = fin;
                                //fin = inicio;
                            }

                            cadena = "";
                            if (linea.Count != 0)
                            {
                                if (linea.Peek() == ' ' || linea.Peek() == '\t')
                                {
                                    linea.Dequeue();
                                    inicio += 2;
                                    fin = inicio;
                                }
                                else
                                {
                                    inicio++;
                                    fin = inicio;
                                }
                            }
                        }
                        else if (linea.Peek() == ' ' || linea.Peek() == '\t')
                        {
                                
                            linea.Dequeue();
                            inicio++;
                            fin = inicio;
                        }
                        else
                        {
                            string caracter = linea.Dequeue().ToString();
                            salida.Add("*** Error en linea " +  llave + "caracter no reconocido: "+ "'"+ caracter +"'");
                            errores.Add("*** Error en linea " + llave + "caracter no reconocido: " + "'" + caracter + "'");
                            correcto = false;
                            fin++;
                            inicio = fin;
                        }



                    }


                }


                llave++;
            } while (archivo.ContainsKey(llave));
            return salida;
        }
        private string validarCadena(string cadena, int llave, int inicio, int final)
        {
            if (cadena.Length < 31)
            {

                if (reservadas.IsMatch(cadena))
                {

                    return   cadena + " en linea " + llave.ToString() + " Cols " + inicio + " - " + final + " es T_reservada";
                }
                else if (booleano.IsMatch(cadena))
                {
                    return   cadena + " en linea " + llave.ToString() + " Cols " + inicio + " - " + final + "es T_ConstanteBooleana";
                }
                else if (cadena == "&&" || cadena == "||")
                {
                    return cadena + " en linea " + llave.ToString() + " Cols " + inicio + " - " + final + "es T_Operador";
                }
                else if (id.IsMatch(cadena))
                {

                    return  cadena + " en linea " + llave.ToString() + " Cols " + inicio + " - " + final + " es T_identificador";
                }
                else if (enterosB10.IsMatch(cadena))
                {

                    return  cadena + " en linea " + llave.ToString()  +" Cols " + inicio + " - " + final+" es T_ConstanteIntB10"+" (valor = " + cadena + ")";
                }
                else if (enterosB16.IsMatch(cadena))
                {
                    
                    //decimal numerohexa = new decimal(Convert.ToInt32(cadena, 16));
                    return cadena + " en linea " + llave.ToString() + " Cols " + inicio + " - " + final +" es T_ConstanteIntB16"+ " (valor = " + cadena + ")";
                }
                else if (decimales.IsMatch(cadena))
                {
                    
                    //decimal numeroDouble = new decimal(Convert.ToDouble(cadena));
                    return cadena + " en linea " + llave.ToString() + " Cols " + inicio + " - " + final +" es T_ConstanteDouble" +" (valor = " + cadena + ")";
                }

                else
                {


                    return "";
                }
            }
            else
            {
                //errores.Add("***Error T_identificador muy largo: " + cadena + " en linea " + llave.ToString() + "\n");
                //correcto = false;
                return "****Error T_identificador muy largo: " + cadena + " en linea " + llave.ToString() + "\n";

            }
        }
    }
}
