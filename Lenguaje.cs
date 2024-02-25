using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*Requerimento 1: Marcar errores sintacticos para variables no declaradas = CUMPLIDO?(PRINTF Y SCANF)
/*Requerimiento 2: Asignación, modifica el valor de la variable, no pasar por alto es ++ y -- = LO DUDO ):
Requerimiento 3: Printf, quitar las comillas, implementar secuencias de escapes /n /t = CUMPLIDO
Requerimiento 4: Modificar el valor de la variable en el scanf y levantar una excepción= LISTO
                    si lo calculado no es un número
Requerimiento 5: Implementar casteo BUSCAR COMO CASTEAR EN C# = NO YA CASI 
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
            foreach (Variable v in variables)// foreach buscar variable clase20/02/24 3 dias de entregar
            {
                if (nombre == v.getNombre())// si existe obtener el tipo de dato de la variable hacer swtich tipo de dato 
                    switch (v.getTipo())
                    {
                        case Variable.TipoDato.Char:
                            v.setValor(nuevovalor % 256);
                            break;
                        case Variable.TipoDato.Int:
                            v.setValor(nuevovalor % 65536);
                            break;
                        case Variable.TipoDato.Float:
                            v.setValor(nuevovalor);
                            break;
                    }
                else
                {
                    v.setValor(nuevovalor);
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
            match(Tipos.Identificador);
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
        // Requerimiento 1: Printf -> printf(cadena(, Identificador)?);
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

                if (!existeVariable(identificador))
                {
                    throw new Error("Sintaxis: la variable " + identificador + " no esta declarada", log, linea);
                }

                Console.WriteLine(valorVariable(identificador));
                match(Tipos.Identificador);
            }

            match(")");
            match(";");
        }

        // Requerimiento 2: Scanf -> scanf(cadena,&Identificador);
        private void Scanf()
        {
            match("scanf");
            match("(");
            match(Tipos.Cadena);
            match(",");
            match("&");
            string identificador = getContenido();

            if (!existeVariable(identificador))
            {
                throw new Error("Sintaxis: la variable " + identificador + " no esta declarada", log, linea);
            }


            string nombre = getContenido();
            match(Tipos.Identificador);
            string valor = Console.ReadLine();
            try
            {
                modificaValor(nombre, float.Parse(valor));
            }
            catch (System.Exception)
            {

                throw new Error("Sintaxis: el valor no es un número", log, linea);
            }
            //modificaValor(nombre, float.Parse(valor));
            match(")");
            match(";");
        }

        //Asignacion -> Identificador (++ | --) | (+= | -=) Expresion | (= Expresion) ;
        private void Asignacion()
        {
            //match(Tipos.Identificador);//1
            if (existeVariable(getContenido()))
            {
                match(Tipos.Identificador);
            }
            else
            {
                throw new Error("Sintaxis: la variable " + getContenido() + " no esta declarada", log, linea);
            }
            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                string operador = getContenido();
                match(Tipos.IncrementoTermino);
                if (operador == "+=" || operador == "-=")
                {
                    Expresion();
                }
            }
            else if (getClasificacion() == Tipos.IncrementoFactor)
            {
                match(Tipos.IncrementoFactor);
                Expresion();
            }
            else
            {
                match("=");
                Expresion();
            }
            //Console.WriteLine(s.Pop());
            match(";");
        }
        //If -> if (Condicion) instruccion | bloqueInstrucciones 
        //      (else instruccion | bloqueInstrucciones)?
        private void If()
        {
            match("if");
            match("(");
            Condicion();
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones();
            }
            else
            {
                Instruccion();
            }
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
        }
        //Condicion -> Expresion operadoRelacional Expresion

        private void Condicion()
        {
            Expresion();
            match(Tipos.OperadorRelacional);
            Expresion();
            s.Pop();
            s.Pop();
        }
        //While -> while(Condicion) bloqueInstrucciones | Instruccion
        private void While()
        {
            match("while");
            match("(");
            Condicion();
            match(")");
            if (getContenido() == "{")
            {
                bloqueInstrucciones();
            }
            else
            {
                Instruccion();
            }
        }
        //Do -> do bloqueInstrucciones | Intruccion while(Condicion);
        private void Do()
        {
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
            Condicion();
            match(")");
            match(";");

        }
        //For -> for(Asignacion Condicion; Incremento) BloqueInstruccones | Instruccion 
        private void For()
        {
            match("for");
            match("(");
            Asignacion();
            Condicion();
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
        }
        //Incremento -> Identificador ++ | --
        private void Incremento()
        {
            match(Tipos.Identificador);//1
            if (getClasificacion() == Tipos.IncrementoTermino)
            {
                match(Tipos.IncrementoTermino);
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
        //MasTermino -> (OperadorTermino Termino)?
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
                // Llamada recursiva para operaciones múltiples
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
                PorFactor(); 
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
                    }

                    float resultado = s.Pop();
                    s.Push(resultado);

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

