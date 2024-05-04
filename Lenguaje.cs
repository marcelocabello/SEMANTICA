using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Tar;
using System.Linq;
using System.Threading.Tasks;

/*Requerimento 1:COLOCAR EL TIPO DE DATO EN ASM DEPDENDINENDO DEL TIPO DE DATO DE LA VARIABLE
Requerimiento 2: cambiar todo a nasm
Requerimiento 3: 
Requerimiento 4: 
COMPLETAR FOR Y ARREGLAR if,else
*/
namespace Semantica
{
    public class Lenguaje : Sintaxis
    {
        List<Variable> variables;
        Stack<float> s;
        int countIF;
        int countDO;
        public Lenguaje()
        {
            variables = new List<Variable>();
            s = new Stack<float>();
            countIF = countDO = 0;
        }
        public Lenguaje(string nombre) : base(nombre)
        {
            variables = new List<Variable>();
            s = new Stack<float>();
            countIF = countDO = 0;
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
            asm.WriteLine("section .text");
            asm.WriteLine("\t global _start");
            asm.WriteLine("\n_start:");
            Main();
            asm.WriteLine("ret");
            imprimeVariables();
        }
        private void imprimeVariables()
        {
            log.WriteLine("Variables: ");
            asm.WriteLine("section .data");
            asm.WriteLine("; VARIABLES");
            foreach (Variable v in variables)
            {
                log.WriteLine(v.getNombre() + " = " + v.getTipo() + " = " + v.getValor());
                asm.WriteLine(v.getNombre() + " dw 0");
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
                if (cadena.Contains("%"))
                {
                    cadena = cadena.Replace("%f", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%d", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%s", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%c", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%i", valorVariable(identificador).ToString());
                    cadena = cadena.Replace("%u", valorVariable(identificador).ToString());
                }
                if (!existeVariable(identificador))
                {
                    throw new Error("de Sintaxis : la variable " + identificador + " no existe", log, linea);
                }


            }

            if (evalua)
            {
                Console.Write(cadena);
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
                    asm.WriteLine("inc dword [" + identificador + "]");
                }
                else if (operador == "--")
                {
                    float valorActual = valorVariable(identificador);
                    valorActual--;
                    modificaValor(identificador, valorActual);
                    asm.WriteLine("dec dword [" + identificador + "]");
                }
                else if (operador == "+=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorIncremento = s.Pop();
                    valorActual += valorIncremento;
                    modificaValor(identificador, valorActual);
                    asm.WriteLine("pop eax");
                    asm.WriteLine("add [" + identificador + "],eax");
                }
                else if (operador == "-=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorIncremento = s.Pop();
                    valorActual -= valorIncremento;
                    modificaValor(identificador, valorActual);
                    asm.WriteLine("pop eax");
                    asm.WriteLine("sub [" + identificador + "],eax");
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
                    asm.WriteLine("pop ebx");
                    asm.WriteLine("mov ebx,[" + identificador+ "]");
                    asm.WriteLine("mul ebx");
                    asm.WriteLine("mov [" + identificador + "], eax");
                }
                else if (operador == "/=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorfactor = s.Pop();
                    valorActual /= valorfactor;
                    modificaValor(identificador, valorActual);
                    asm.WriteLine("pop ebx");
                    asm.WriteLine("mov ebx, [" + identificador+ "]");
                    asm.WriteLine("div ebx");
                    asm.WriteLine("mov [" + identificador + "], eax");
                }
                else if (operador == "%=")
                {
                    float valorActual = valorVariable(identificador);
                    Expresion();
                    float valorfactor = s.Pop();
                    valorActual %= valorfactor;
                    modificaValor(identificador, valorActual);
                    asm.WriteLine("pop ebx");
                    asm.WriteLine("mov eax, [" + identificador+ "]");
                    asm.WriteLine("div ebx");
                    asm.WriteLine("mov [" + identificador + "], edx");
                }
                //mover modifica afuera para poner if evalua como el profe
            }
            else
            {
                match("=");
                Expresion();
                float nuevoValor = s.Pop();
                modificaValor(identificador, nuevoValor);
                asm.WriteLine("pop eax");
            }
            asm.WriteLine("mov [" + identificador + "], eax");

            match(";");
        }


        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If(bool evaluaif)
        {
            match("if");
            match("(");
            string etiqueta = "EtiquetaIF" + (++countIF)+":";
            bool evalua = Condicion(etiqueta) && evaluaif;
            match(")");
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
                    bloqueInstrucciones(!evalua);
                }
                else
                {
                    Instruccion(!evalua);
                }
            }
            asm.WriteLine(etiqueta);
        }

        //Condicion -> Expresion operadoRelacional Expresion

        private bool Condicion(string etiqueta)
        {
            Expresion();
            string operador = getContenido();//28 febrero
            match(Tipos.OperadorRelacional);
            Expresion();
            float E2 = s.Pop();
            asm.WriteLine("pop ebx");
            float E1 = s.Pop();
            asm.WriteLine("pop ebx");
            asm.WriteLine("cmp eax, ebx");
            switch (operador)
            {
                case ">":
                    asm.WriteLine("jle " + etiqueta);
                    return E1 > E2; //el mismo operador regresa falso o verdadero en lugar de usar if
                case ">=":
                    asm.WriteLine("jl " + etiqueta);
                    return E1 >= E2;
                case "<":
                    asm.WriteLine("jge " + etiqueta);
                    return E1 < E2;
                case "<=":
                    asm.WriteLine("jg " + etiqueta);
                    return E1 <= E2;
                case "==":
                    asm.WriteLine("jne " + etiqueta);
                    return E1 == E2;
                default:
                    asm.WriteLine("je " + etiqueta);
                    return E1 != E2;
            }
        }
        //While -> while(Condicion) bloqueInstrucciones | Instruccion
        private void While(bool evaluawhile)
        {
            match("while");
            match("(");
            bool evalua = true;
            int counttmp = ccontar - 1;
            //string contenido = getContenido();

            do
            {
                evalua = Condicion("") && evaluawhile;
                match(")");

                switch (getContenido())
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
            string etiqueta = "EtiquetaDO" + (++countDO);
            asm.WriteLine(etiqueta + ":");
            match("do");
            bool evalua = true;
            int counttmp = ccontar - 1;
            //string contenido = getContenido();
            do
            {
                switch (getContenido())
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
                evalua = Condicion(etiqueta) && evaluado;
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

                evaluafor = Condicion("") && evalua;
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
                asm.WriteLine("pop ebx");
                float N1 = s.Pop();
                asm.WriteLine("pop eax");
                switch (operador)
                {
                    case "+":
                        asm.WriteLine("add eax, eax");
                        asm.WriteLine("push ebx");
                        s.Push(N1 + N2);
                        break;
                    case "-":
                        asm.WriteLine("sub eax, ebx");
                        asm.WriteLine("push eax");
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
                asm.WriteLine("pop ebx");
                float N1 = s.Pop();
                asm.WriteLine("pop eax");
                switch (operador)
                {
                    case "*":
                        asm.WriteLine("mul ebx");
                        asm.WriteLine("push eax");
                        s.Push(N1 * N2);
                        break;
                    case "/":
                        asm.WriteLine("div ebx");
                        asm.WriteLine("push eax");
                        s.Push(N1 / N2);
                        break;
                    case "%":
                        asm.WriteLine("div ebx");
                        asm.WriteLine("push edx");
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
                asm.WriteLine("mov eax, [" + getContenido()+"]");
                asm.WriteLine("push eax");
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
                    float valorCaster = s.Pop();


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
                            /*ase "float":
                                s.Push(s.Pop());
                                break;*/
                    }
                    asm.WriteLine("mov eax,[" + valorCaster+ "]");
                    asm.WriteLine("push eax");
                    s.Push(valorCaster);


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