using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Threading.Tasks;

/*Requerimento 1:evalua el else así como el if, do 40 puntos, while con 40 puntos	|| IF Y ELSE SE IMPRIMEN JUNTOS =N0
como regresar en el archivo de texto para verificar las iteraciones
Requerimiento 2: incrementar la variable del for (incremento) al final de la ejecución|| HACE UNO DE MAS
Requerimiento 3: hacer do||listo
Requerimiento 4: hacer while|listo
*/
namespace Semantica
{
    public class Lenguaje : Sintaxis
    {
        List<Variable> variables;
        Stack<float> s;
        public Lenguaje()
        {
            variables = new List<Variable>();
            s = new Stack<float>();
        }
        public Lenguaje(string nombre) : base(nombre)
        {
            variables = new List<Variable>();
            s = new Stack<float>();
        }
        //Programa  -> Librerias? Variables? Main
        public void Programa()
        {
            if (getContenido() == "#")
            {
                Librerias();
            }
            if (getClasificacion() == Tipos.tipoDatos)
            {
                Variables();
            }
            Main();
            imprimeVariables();
        }
        private void imprimeVariables()
        {
            log.WriteLine("Variables: ");
            foreach (Variable v in variables)
            {
                log.WriteLine(v.getNombre() + " = " + v.getTipo() + " = " + v.getValor());
            }
        }
        private void imprimeStack()
        {
            Console.WriteLine("Stack: ");
            foreach (float valor in s)
            {
                Console.WriteLine(valor);
            }
        }
        private bool existeVariable(string nombre)
        {
            foreach (Variable v in variables)
            {
                if (nombre == v.getNombre())
                {
                    return true;
                }
            }
            return false;
        }
        private float valorVariable(string nombre)
        {
            foreach (Variable v in variables)
            {
                if (nombre == v.getNombre())
                {
                    return v.getValor();
                }
            }
            return 0;
        }
        private void modificaValor(string nombre, float nuevovalor)
        {
            foreach (Variable v in variables)
            {
                if (nombre == v.getNombre())
                {
                    // si existe obtener el tipo de dato de la variable hacer switch tipo de dato 
                    //hacer if aqui para que no se salga del rango
                    switch (v.getTipo())
                    {
                        case Variable.TipoDato.Char:
                            if (nuevovalor < 0 || nuevovalor > 255)
                            {
                                // en lugar de sintaxis es semantica
                                throw new Error("Semantica: el valor asignado a la variable " + nombre + " excede el rango permitido para tipo char", log, linea);
                            }
                            v.setValor(nuevovalor % 256);
                            break;
                        case Variable.TipoDato.Int:
                            if (nuevovalor < 0 || nuevovalor > 65535)
                            {
                                throw new Error("Semantica: el valor asignado a la variable " + nombre + " excede el rango permitido para tipo int", log, linea);
                            }
                            v.setValor(nuevovalor % 65536);
                            break;
                        case Variable.TipoDato.Float:
                            v.setValor(nuevovalor);
                            break;
                    }
                    return;
                }
            }
        }


        //Librerias -> #include<identificador(.h)?> Librerias?
        private void Librerias()
        {
            match("#");
            match("include");
            match("<");
            match(Tipos.Identificador);
            if (getContenido() == ".")
            {
                match(".");
                match("h");
            }
            match(">");
            if (getContenido() == "#")
            {
                Librerias();
            }
        }
        //Variables -> tipoDato listaIdentificadores; Variables?
        private void Variables()
        {
            Variable.TipoDato tipo = Variable.TipoDato.Char;
            switch (getContenido())
            {
                case "int":
                    tipo = Variable.TipoDato.Int;
                    break;
                case "float":
                    tipo = Variable.TipoDato.Float;
                    break;
                case "char":
                    tipo = Variable.TipoDato.Char;
                    break;
            }
            match(Tipos.tipoDatos);
            listaIdentificadores(tipo);
            match(";");
            if (getClasificacion() == Tipos.tipoDatos)
            {
                Variables();
            }
        }
        //listaIdentificadores -> Identificador (,listaIdentificadores)?
        private void listaIdentificadores(Variable.TipoDato tipo)
        {
            string nombre = getContenido();
            match(Tipos.Identificador);//1
            if (!existeVariable(nombre))
            {
                variables.Add(new Variable(nombre, tipo));
            }
            else
            {
                throw new Error("de Sintaxis : la variable " + nombre + " ya existe", log, linea);
            }
            if (getContenido() == ",")
            {
                match(",");
                listaIdentificadores(tipo);
            }
        }
        //bloqueInstrucciones -> { listaIntrucciones? }
        private void bloqueInstrucciones(bool evalua)
        {
            match("{");
            if (getContenido() != "}")
            {
                ListaInstrucciones(evalua);
            }
            match("}");
        }
        //ListaInstrucciones -> Instruccion ListaInstrucciones?
        private void ListaInstrucciones(bool evalua)
        {
            Instruccion(evalua);
            if (getContenido() != "}")
            {
                ListaInstrucciones(evalua);
            }
        }
        //Instruccion -> Printf | Scanf | If | While | do while | For | Asignacion
        private void Instruccion(bool evalua)
        {
            if (getContenido() == "printf")
            {
                Printf(evalua);
            }
            else if (getContenido() == "scanf")
            {
                Scanf(evalua);
            }
            else if (getContenido() == "if")
            {
                If(evalua);
            }
            else if (getContenido() == "while")
            {
                While(evalua);
            }
            else if (getContenido() == "do")
            {
                Do(evalua);
            }
            else if (getContenido() == "for")
            {
                For(evalua);
            }
            else
            {
                Asignacion(evalua);
            }
        }
        //    Requerimiento 1: Printf -> printf(cadena(, Identificador)?);

        private void Printf(bool evalua)
        {
            match("printf");
            match("(");

            string cadena = getContenido();
            cadena = cadena.Replace("\\n", "\n").Replace("\\t", "\t").Trim('"');
            match(Tipos.Cadena);

            if (getContenido() == ",")
            {
                match(",");
                string identificador = getContenido();
                match(Tipos.Identificador);
                if (!existeVariable(identificador))
                {
                    throw new Error("de Sintaxis : la variable " + identificador + " no existe", log, linea);
                }
                if (cadena.Contains("%"))
                {
                    cadena = cadena.Replace("%f", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%d", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%s", valorVariable(identificador).ToString());
                }
            }

            if (evalua)
            {
                Console.WriteLine(cadena);
            }

            match(")");
            match(";");
        }

        // Requerimiento 2: Scanf -> scanf(cadena,&Identificador);
        private void Scanf(bool evalua)
        {
            match("scanf");
            match("(");
            match(Tipos.Cadena);
            match(",");
            match("&");

            string identificador = getContenido();
            match(Tipos.Identificador);//1

            if (!existeVariable(identificador))
            {
                throw new Error("Sintaxis: la variable " + identificador + " no esta declarada", log, linea);
            }

            // No imprimas un mensaje adicional antes de leer la entrada del usuario
            if (evalua)
            {
                try
                {
                    string valor = Console.ReadLine();
                    float nuevoValor = float.Parse(valor);
                    modificaValor(identificador, nuevoValor);
                }
                catch (System.FormatException)
                {
                    throw new Error("Sintaxis: el valor ingresado no es un número válido", log, linea);
                }
            }
            match(")");
            match(";");
        }

        // Asignacion -> Identificador (+= IncrementoTermino)? (= Expresion) ;
        private void Asignacion(bool evalua)
        {
            string identificador = getContenido();
            match(Tipos.Identificador);//1

            if (!existeVariable(identificador))
            {
                throw new Error("Sintaxis: la variable " + identificador + " no está declarada", log, linea);
            }

            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                string operador = getContenido();
                match(Tipos.IncrementoTermino);
                if (operador == "++")
                {
                    float valorActual = valorVariable(identificador);
                    valorActual++;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "--")
                {
                    float valorActual = valorVariable(identificador);
                    valorActual--;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "+=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorIncremento = s.Pop();
                    valorActual += valorIncremento;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "-=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorIncremento = s.Pop();
                    valorActual -= valorIncremento;
                    modificaValor(identificador, valorActual);
                }
                //modifica valor agregar al final, aqui para optimizar

            }
            else if (getClasificacion() == Tipos.IncrementoFactor)
            {
                string operador = getContenido();
                match(Tipos.IncrementoFactor);
                if (operador == "*=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorfactor = s.Pop();
                    valorActual *= valorfactor;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "/=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorfactor = s.Pop();
                    valorActual /= valorfactor;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "%=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorfactor = s.Pop();
                    valorActual %= valorfactor;
                    modificaValor(identificador, valorActual);
                }
                //mover modifica afuera para poner if evalua como el profe
            }
            else
            {
                match("=");
                Expresion();
                float nuevoValor = s.Pop();
                modificaValor(identificador, nuevoValor);
            }

            match(";");
        }


        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If(bool evaluaif)
        {
            match("if");
            match("(");
            bool evalua = Condicion() && evaluaif;
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones(evalua==true);
            }
            else
            {
                Instruccion(evalua==true);
            }
            if (getContenido() == "else")
            {
                match("else");
                if (getContenido() == "if")
                {
                    If(evaluaif==false);
                }
                else if (getContenido() == "{")
                {
                    bloqueInstrucciones(evalua==false);
                }
                else
                {
                    Instruccion(evalua==false);
                }
            }
        }

        //Condicion -> Expresion operadoRelacional Expresion

        private bool Condicion()
        {
            Expresion();
            string operador = getContenido();//28 febrero
            match(Tipos.OperadorRelacional);
            Expresion();
            float E2 = s.Pop();
            float E1 = s.Pop();
            switch (operador)
            {
                case ">": return E1 > E2; //el mismo operador regresa falso o verdadero en lugar de usar if
                case ">=": return E1 >= E2;
                case "<": return E1 < E2;
                case "<=": return E1 <= E2;
                case "==": return E1 == E2;
                default: return E1 != E2;
            }
        }
        //While -> while(Condicion) bloqueInstrucciones | Instruccion
        private void While(bool evaluawhile)
        {
            match("while");
            match("(");
            bool evalua = true;
            int counttmp = ccontar - 1;
            string contenido = getContenido();

            do
            {
                evalua = Condicion() && evaluawhile;
                match(")");

                switch (contenido)
                {
                    case "{":
                        bloqueInstrucciones(true);
                        break;
                    default:
                        Instruccion(true);
                        break;
                }

                if (evalua)
                {
                    ccontar = counttmp;
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(counttmp, SeekOrigin.Begin);
                    nextToken();
                }

            } while (evalua);
        }

        //Do -> do bloqueInstrucciones | Intruccion while(Condicion);
        private void Do(bool evaluado)
        {
            match("do");
            bool evalua = true;
            int counttmp = ccontar - 1;
            string contenido = getContenido();
            do
            {
                switch (contenido)
                {
                    case "{":
                        bloqueInstrucciones(true);
                        break;
                    default:
                        Instruccion(true);
                        break;
                }

                match("while");
                match("(");
                evalua = Condicion() && evaluado;
                match(")");
                match(";");

                if (evalua)
                {
                    ccontar = counttmp;
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(counttmp, SeekOrigin.Begin);
                    nextToken();
                }

            } while (evalua);
        }
        //For -> for(Asignacion Condicion; Incremento) BloqueInstruccones | Instruccion 
        private void For(bool evalua)
        {
            match("for");
            match("(");
            Asignacion(evalua);
            string variable = getContenido();
            bool evaluafor = true;
            int counttmp = ccontar - 1;
            int lineatmp = linea;
            do
            {

                evaluafor = Condicion() && evalua;
                match(";");
                Incremento(evaluafor);
                match(")");
                if (getContenido() == "{")
                {
                    bloqueInstrucciones(evaluafor);
                }
                else
                {
                    Instruccion(evaluafor);
                }
                if (evaluafor)
                {
                    ccontar = counttmp - variable.Length;
                    linea = lineatmp;
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(ccontar, SeekOrigin.Begin);
                    nextToken();
                    modificaValor(variable, valorVariable(variable) + 1);


                }

            } while (evaluafor);
        }

        //Incremento -> Identificador ++ | --
        private int Incremento(bool evalua)
        {
            string nombre = getContenido();
            
            match(Tipos.Identificador);
            if (!existeVariable(nombre))
            {
                throw new Error("de Sintaxis : la variable " + nombre + " no existe", log, linea);
            }
            if (getContenido() == "++")
            {
                match("++");
                return 1;
            }

            else
            {
                match("--");
                return -1;
            }
        }
        //Main      -> void main() bloqueInstrucciones
        private void Main()
        {
            match("void");
            match("main");
            match("(");
            match(")");
            bloqueInstrucciones(true);
        }
        //Expresion -> Termino MasTermino
        private void Expresion()
        {
            Termino();
            MasTermino();
        }

        // MasTermino -> (OperadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == Tipos.OperadorTermino)
            {
                string operador = getContenido();
                match(Tipos.OperadorTermino);
                Termino();
                // Realizar la operación correspondiente
                float N2 = s.Pop();
                float N1 = s.Pop();
                switch (operador)
                {
                    case "+":
                        s.Push(N1 + N2);
                        break;
                    case "-":
                        s.Push(N1 - N2);
                        break;
                }

            }
        }
        private void Termino()
        {
            Factor();
            PorFactor();

        }
        // PorFactor -> (OperadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == Tipos.OperadorFactor)
            {
                string operador = getContenido();
                match(Tipos.OperadorFactor);
                Factor();
                // Realizar la operación correspondiente
                float N2 = s.Pop();
                float N1 = s.Pop();
                switch (operador)
                {
                    case "*":
                        s.Push(N1 * N2);
                        break;
                    case "/":
                        s.Push(N1 / N2);
                        break;
                    case "%":
                        s.Push(N1 % N2);
                        break;
                }

            }
        }

        // Factor -> numero | identificador | (Expresion)
        private void Factor()
        {
            if (getClasificacion() == Tipos.Numero)
            {
                //Console.Write(getContenido());
                //s.Push(float.Parse(getContenido()));
                s.Push(float.Parse(getContenido())); // Se agregó esta línea
                match(Tipos.Numero);
            }
            else if (getClasificacion() == Tipos.Identificador)
            {
                //Console.Write(getContenido());
                s.Push(valorVariable(getContenido()));
                if (!existeVariable(getContenido()))
                {
                    throw new Error("Sintaxis: la variable " + getContenido() + " no esta declarada", log, linea);
                }
                match(Tipos.Identificador);//1

            }
            else
            {
                match("(");
                if (getClasificacion() == Tipos.tipoDatos)
                {
                    string tipo = getContenido();
                    match(Tipos.tipoDatos);
                    match(")");
                    Expresion();

                    // Pop: sacar lo de la expresión, dividir lo del pop dependiendo del tipo de dato,
                    // luego el resultado meter nuevamente al stack

                    switch (tipo)
                    {
                        case "char":
                            s.Push(s.Pop() % 256);
                            break;
                        case "int":
                            s.Push(s.Pop() % 65536);
                            break;
                        case "float":
                            s.Push(s.Pop());
                            break;
                    }


                }
                else
                {
                    Expresion();
                    match(")");
                }
            }
        }

    }
}