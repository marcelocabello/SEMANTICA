using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*Requerimento 1: Marcar errores sintacticos para variables no declaradas = CUMPLIDO
/*Requerimiento 2: Asignación, modifica el valor de la variable, no pasar por alto es ++ y -- = CUMPLIDO, creo
Requerimiento 3: Printf, quitar las comillas, implementar secuencias de escapes /n /t = CUMPLIDO
Requerimiento 4: Modificar el valor de la variable en el scanf y levantar una excepción= CUMPLIDO
                    si lo calculado no es un número
Requerimiento 5: Implementar casteo BUSCAR COMO CASTEAR EN C# = X NO 
*/
namespace Semantica
{
    public class Lenguaje : Sintaxis
    {
        List<Variable> variables;
        Stack<float> s;
        int countIF;
        int countDO;

        int str_msg;
        int str_num;
        int ifct;
        int whilect;
        int doct;
        int forct;
        public Lenguaje()
        {
            variables = new List<Variable>();
            s = new Stack<float>();
            ifct = whilect = doct = forct = str_num = str_msg = countIF = countDO = 0;
        }
        public Lenguaje(string nombre) : base(nombre)
        {
            variables = new List<Variable>();
            s = new Stack<float>();
            ifct = whilect = doct = forct = str_num = str_msg = countIF = countDO = 0;
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
            asm.WriteLine("section .data:");

            imprimeVariables();
            asm.WriteLine("section .text");
            asm.WriteLine("\t global _start");
            asm.WriteLine("\n_start:\n");

            Main();
            //asm.WriteLine("ret");
            //imprimeVariables();
            Finasm();
        }
        private void Finasm()
        {
            asm.WriteLine(";FIN PROGRAMA:");
            asm.WriteLine("\nmov eax, 1");
            asm.WriteLine("xor ebx, ebx ");
            asm.WriteLine("int 0x80");
        }
        private void imprimeVariables()
        {
            log.WriteLine("Variables: ");
            asm.WriteLine("; VARIABLES");
            foreach (Variable v in variables)
            {
                log.WriteLine(v.getNombre() + " = " + v.getTipo() + " = " + v.getValor());
                switch (v.getTipo())
                {
                    case Variable.TipoDato.Char:
                        asm.WriteLine(v.getNombre() + " db 0");
                        break;
                    case Variable.TipoDato.Int:
                        asm.WriteLine(v.getNombre() + " dw 0");
                        break;
                    case Variable.TipoDato.Float:
                        asm.WriteLine(v.getNombre() + " dd 0");
                        break;
                }
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
        private void bloqueInstrucciones()
        {
            match("{");
            if (getContenido() != "}")
            {
                ListaInstrucciones();
            }
            match("}");
        }
        //ListaInstrucciones -> Instruccion ListaInstrucciones?
        private void ListaInstrucciones()
        {
            Instruccion();
            if (getContenido() != "}")
            {
                ListaInstrucciones();
            }
        }
        //Instruccion -> Printf | Scanf | If | While | do while | For | Asignacion
        private void Instruccion()
        {
            if (getContenido() == "printf")
            {
                Printf();
            }
            else if (getContenido() == "scanf")
            {
                Scanf();
            }
            else if (getContenido() == "if")
            {
                If();
            }
            else if (getContenido() == "while")
            {
                While();
            }
            else if (getContenido() == "do")
            {
                Do();
            }
            else if (getContenido() == "for")
            {
                For();
            }
            else
            {
                Asignacion();
            }
        }
        //    Requerimiento 1: Printf -> printf(cadena(, Identificador)?);

        private void Printf()
        {
            match("printf");
            match("(");

            string cadena = getContenido();
            Console.WriteLine(cadena.Replace("\\n", "\n").Replace("\\t", "\t").Trim('"'));
            match(Tipos.Cadena);

            if (getContenido() == ",")
            {
                match(",");
                string identificador = getContenido();
                match(Tipos.Identificador);//1
                if (!existeVariable(identificador))
                {
                    throw new Error("Sintaxis: la variable " + identificador + " no esta declarada", log, linea);
                }

                Console.WriteLine(valorVariable(identificador));

            }

            match(")");
            match(";");
        }

        // Requerimiento 2: Scanf -> scanf(cadena,&Identificador);
        private void Scanf()
        {
            asm.WriteLine(";LECTURA " + (++str_num) + ": ");
            match("scanf");
            match("(");
            match(Tipos.Cadena);
            match(",");
            match("&");

            string identificador = getContenido();
            string cadena = getContenido();
            string mensaje = "str_msg" + str_num;
            match(Tipos.Identificador);//1

            if (!existeVariable(identificador))
            {
                throw new Error("Sintaxis: la variable " + identificador + " no esta declarada", log, linea);
            }
            try//para git
            {
                string valor = Console.ReadLine();
                float nuevoValor = float.Parse(valor);
                modificaValor(identificador, nuevoValor);
            }
            catch (System.FormatException)
            {
                throw new Error("Sintaxis: el valor ingresado no es un número válido", log, linea);
            }

            asm.WriteLine("\nmov eax, 3 ");
            asm.WriteLine("mov ebx, 2 ");
            asm.WriteLine("lea ecx, [" + identificador + "]");
            asm.WriteLine("mov edx, " + cadena.Length);
            asm.WriteLine("int 0x80\n");
            match(")");
            match(";");
        }


        // Asignacion -> Identificador (+= IncrementoTermino)? (= Expresion) ;
        private void Asignacion()
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
                    asm.WriteLine("mov ebx, " + identificador);
                    asm.WriteLine("imul ebx");
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
                    asm.WriteLine("mov ebx, " + identificador);
                    asm.WriteLine("idiv ebx");
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
                    asm.WriteLine("mov eax, " + identificador);
                    asm.WriteLine("idiv ebx");
                    asm.WriteLine("mov [" + identificador + "], edx");
                }
            }
            else
            {
                match("=");
                Expresion();
                float nuevoValor = s.Pop();
                modificaValor(identificador, nuevoValor);
                asm.WriteLine("pop eax");
            }

            match(";");
        }


        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If()
        {
            asm.WriteLine(";IF");
            string etiquetaIF = "EtiquetaIF" + (++ifct);
            string etiquetafinIF = "EtiquetafinIF" + ifct;
            match("if");
            match("(");
            Condicion(etiquetaIF);
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones();
            }
            else
            {
                Instruccion();
            }
            asm.WriteLine("jmp " +etiquetafinIF);
			asm.WriteLine(etiquetaIF+ ":");
            if (getContenido() == "else")
            {
                match("else");
                if (getContenido() == "{")
                {
                    bloqueInstrucciones();
                }
                else
                {
                    Instruccion();
                }
            }
            asm.WriteLine(etiquetafinIF + ":");
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
            asm.WriteLine("pop eax");
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
        private void While()
        {
            asm.WriteLine(";WHILE");
            string etiquetaWL = "EtiquetaWl" + (++whilect);
            string etiquetafinWL = "EtiquetaWlFin" + whilect;
            asm.WriteLine(etiquetaWL + ":");
            match("while");
            match("(");
            Condicion(etiquetafinWL);
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones();
            }
            else
            {
                Instruccion();
            }
            asm.WriteLine("jmp " + etiquetaWL);
            asm.WriteLine(etiquetafinWL + ":");
        }
        //Do -> do bloqueInstrucciones | Intruccion while(Condicion);
        private void Do()
        {
            asm.WriteLine(";DO");
            string etiquetaDO = "EtiquetaDO" + (++countDO);
            string etiquetafinDO = "EtiquetafinDO" + countDO;
            asm.WriteLine(etiquetaDO + ":");
            match("do");

            if (getContenido() == "{")
            {
                bloqueInstrucciones();
            }
            else
            {
                Instruccion();
            }
            match("while");
            match("(");
            Condicion(etiquetafinDO);
            match(")");
            match(";");
            asm.WriteLine("jmp " + etiquetaDO);
            asm.WriteLine(etiquetafinDO + ":");

        }
        //For -> for(Asignacion Condicion; Incremento) BloqueInstruccones | Instruccion 
        private void For()
        {
            asm.WriteLine(";FOR");
            string etiquetaFOR = "EtiquetaFOR" + (++forct);
			string etiquetaFINFOR = "EtiquetafinFOR" + forct;
            asm.WriteLine(etiquetaFOR + ":");
            
            match("for");
            match("(");
            Asignacion();
            Condicion(etiquetaFINFOR);
            match(";");
            Incremento();
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones();
            }
            else
            {
                Instruccion();
            }
            
            asm.WriteLine("jmp " + etiquetaFOR);
            asm.WriteLine(etiquetaFINFOR + ":");
        }
        //Incremento -> Identificador ++ | --
        private int Incremento()
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
                asm.WriteLine("mov eax, [" + nombre + "]");
				asm.WriteLine("inc eax");
				asm.WriteLine("mov [" + nombre+ "], eax");
                return 1;
            }

            else
            {
                match("--");
                asm.WriteLine("mov eax, [" + nombre + "]");
				asm.WriteLine("dec eax");
				asm.WriteLine("mov [" + nombre+ "], eax");
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
            bloqueInstrucciones();
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
                        asm.WriteLine("mov eax, ebx");
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
                        asm.WriteLine("imul ebx");
                        asm.WriteLine("push eax");
                        s.Push(N1 * N2);
                        break;
                    case "/":
                        asm.WriteLine("idiv ebx");
                        asm.WriteLine("push eax");
                        s.Push(N1 / N2);
                        break;
                    case "%":
                        asm.WriteLine("idiv ebx");
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
                asm.WriteLine("mov eax," + getContenido());
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
                    asm.WriteLine("mov eax," + valorCaster);
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

