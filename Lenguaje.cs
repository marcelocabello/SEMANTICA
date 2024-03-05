using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Threading.Tasks;

/*Requerimento 1:evalua el else así como el if, do 40 puntos, while con 40 puntos	
como regresar en el archivo de texto para verificar las iteraciones
Requerimiento 2: incrementar la variable del for (incremento) al final de la ejecución
Requerimiento 3: hacer do
Requerimiento 4: hacer while
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
            Main(true);
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
            Console.WriteLine(cadena.Replace("\\n", "\n").Replace("\\t", "\t").Trim('"'));
            match(Tipos.Cadena);
            if (evalua)
            {
                Console.Write(cadena);
            }
            if (getContenido() == ",")
            {
                match(",");
                string identificador = getContenido();
                match(Tipos.Identificador);//1
                if (!existeVariable(identificador))
                {
                    throw new Error("Sintaxis: la variable " + identificador + " no esta declarada", log, linea);
                }
                else
                {
                    if (evalua)
                    {
                        Console.WriteLine(valorVariable(identificador));
                    }
                }

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

            //Console.Write("Ingrese un valor para " + identificador + ": ");
            if (evalua)
            {
                string valor = Console.ReadLine();
                try
                {

                    float nuevoValor = float.Parse(valor);
                    modificaValor(identificador, nuevoValor);
                    //match(")");
                    //match(";");
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
                    Expresion(evalua);
                    float valorIncremento = s.Pop();
                    valorActual += valorIncremento;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "-=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion(evalua);
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
                    Expresion(evalua);
                    float valorfactor = s.Pop();
                    valorActual *= valorfactor;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "/=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion(evalua);
                    float valorfactor = s.Pop();
                    valorActual /= valorfactor;
                    modificaValor(identificador, valorActual);
                }
                else if (operador == "%=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion(evalua);
                    float valorfactor = s.Pop();
                    valorActual %= valorfactor;
                    modificaValor(identificador, valorActual);
                }
                //mover modifica afuera para poner if evalua como el profe
            }
            else
            {
                match("=");
                Expresion(evalua);
                float nuevoValor = s.Pop();
                modificaValor(identificador, nuevoValor);
            }

            match(";");
        }


        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If(bool evaluaif)
        {//modificacion 28 de febrero
            match("if");
            match("(");
            bool evalua = Condicion() && evaluaif;
            match(")");
            //Console.WriteLine(evalua);
            if (getContenido() == "{")
            {

                bloqueInstrucciones(evalua);

            }
            else
            {
                Instruccion(evalua);
            }
            if (getContenido() == "else")
            {
                match("else");
                if (getContenido() == "{")
                {
                    bloqueInstrucciones(evalua);
                }
                else
                {
                    Instruccion(evalua);
                }
            }
        }
        //Condicion -> Expresion operadoRelacional Expresion

        private bool Condicion(bool evalua)
        {
            Expresion(evalua);
            string operador = getContenido();//28 febrero
            match(Tipos.OperadorRelacional);
            Expresion(evalua);
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
        private void While(bool evalua)
        {
            match("while");
            match("(");
            Condicion(evalua);
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones(evalua);
            }
            else
            {
                Instruccion(evalua);
            }
        }
        //Do -> do bloqueInstrucciones | Intruccion while(Condicion);
        private void Do(bool evalua)
        {
            match("do");
            if (getContenido() == "{")
            {
                bloqueInstrucciones(evalua);
            }
            else
            {
                Instruccion(evalua);
            }
            match("while");
            match("(");
            Condicion(evalua);
            match(")");
            match(";");

        }
        //For -> for(Asignacion Condicion; Incremento) BloqueInstruccones | Instruccion 
        private void For(bool evalua)
        {
            match("for");
            match("(");
            Asignacion(evalua);
            int counttmp = ccount- 1 - valorVariable.Length;
            int lineatmp = linea;
            bool evaluafor = true;
            string variable = getContenido();
            Console.WriteLine(variable);
            do
            {
               
                

                evaluafor = Condicion() && evalua;
                match(";");
                Incremento();//quitar el evalua
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
                    modificaValor(variable, valorVariable(variable) + 1);
                    ccount = counttmp - variable.Length;
                    linea = lineatmp;
                    Console.WriteLine(getContenido());
                    archivo.DiscardBufferedData();
                    archivo.BaseStream.Seek(ccount, SeekOrigin.Begin);
                    nextToken();
                    
                }
            } while (evaluafor);
        }
        //Incremento -> Identificador ++ | --
        private void Incremento()
        {
            string nombre = getContenido();
            bool incremento = false;
            match(Tipos.Identificador);
            if (!existeVariable(nombre))
            {
                throw new Error("de Sintaxis : la variable " + nombre + " no existe", log, linea);
            }
            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                if (getContenido() == "++")
                {
                    incremento = true;
                }
                match(Tipos.IncrementoTermino);
            }
            if (incremento)
                return 1;
            return -1;
        }
        //Main      -> void main() bloqueInstrucciones
        private void Main(bool evalua)
        {
            match("void");
            match("main");
            match("(");
            match(")");
            bloqueInstrucciones(evalua);
        }
        //Expresion -> Termino MasTermino
        private void Expresion(bool evalua)
        {
            Termino(evalua);
            MasTermino(evalua);
        }

        // MasTermino -> (OperadorTermino Termino)?
        private void MasTermino(bool evalua)
        {
            if (getClasificacion() == Tipos.OperadorTermino)
            {
                string operador = getContenido();
                match(Tipos.OperadorTermino);
                Termino(evalua);
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
        private void Termino(bool evalua)
        {
            Factor(evalua);
            PorFactor(evalua);

        }
        // PorFactor -> (OperadorFactor Factor)?
        private void PorFactor(bool evalua)
        {
            if (getClasificacion() == Tipos.OperadorFactor)
            {
                string operador = getContenido();
                match(Tipos.OperadorFactor);
                Factor(evalua);
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
        private void Factor(bool evalua)
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
                    Expresion(evalua);

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
                    Expresion(evalua);
                    match(")");
                }
            }
        }

    }
}

