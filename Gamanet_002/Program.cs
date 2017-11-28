using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Gamanet_002
{

    class Stream : StreamReader
    {   
        public int Peak()
        {
            return base.Peek();
        }
        public int Read()
        {
            return base.Read();
        }
        public Stream() : base(Console.OpenStandardInput())
        {
            
        }  
    }

    class Context
    {
        String command;  
        String parameter; //at the time of program execution it is populated with parameter
        String packet;   //at the time of program execution it is populated with full packet

        public void setPacket(String packet) { this.packet = packet; }
        public String getPacket() { return this.packet; }

        public void setCommand(String command) { this.command = command; }
        public String getCommand() { return this.command; }

        public void setParameter(String parameter) { this.parameter = parameter; }
        public String getParameter() { return this.parameter; }
    }



//C:\Users\AdanaC\source\repos\Gamanet_002\Gamanet_002\bin\Debug;   < test.txt

    class Program
    {   
        private delegate bool Parse(Context context, Stream stream);
        static void Main(string[] args)
        {
            // starts the program
            run();
        }

        
        static void run()
        {
            Console.WriteLine();
           
            Context context = new Context();
            Stream stream = new Stream();

            // creates fixed order of checking methods call.
            Parse[] parsers = new Parse[] {  checkBegin, checkComand, checkSeparator, checkParams, checkSeparator, checkEnding};
            
            try
            {
                while (true)
                {
                    bool ACKpacket = true;
                    foreach (Parse p in parsers)
                    {
                        if (!p(context, stream)) { ACKpacket = false; break; }
                    }

                    if (ACKpacket) {
                        Console.WriteLine("ACK   "+ context.getPacket());
                        executeCommand(context);
                    }
                    else { Console.WriteLine("NACK  " + context.getPacket()); }
                    context.setPacket("");   
                }
            }
            catch(EndOfStreamException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        // checks the first symbol of the packet
        private static bool checkBegin(Context context, Stream stream)
        {
            char beginSymbol = 'P'; // first symbol of the packet
            return checkedRead(beginSymbol, context, stream);
        }

        // checks the Command symbol. There are only two commands: text and sound, if the symbol is correct than it returns true 
        private static bool checkComand(Context context, Stream stream)
        {
            char textCommandSymbol = 'T'; 
            char soundCommandSymbol = 'S';
            int symbol = stream.Read();
            context.setPacket(context.getPacket() + (char)symbol); // creating the packet

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }


            // checks if the command is text or sound 
            if (((char)symbol == textCommandSymbol) || ((char)symbol == soundCommandSymbol)) { context.setCommand(((char)symbol) + ""); return true; }
            else {return false; }
        }

        //checks the separator symbol. It should be ':', if the symbol is correct than it returns true
        private static bool checkSeparator(Context context, Stream stream)
        {
            char separatorSymbol = ':';
            
            return checkedRead(separatorSymbol, context, stream); 
        }

        //checks the Parameters symbols and creates a string. Checks if he string corresponds to the command.
        private static bool checkParams(Context context, Stream stream)
        {
            bool normalSymbolInPacket = true;
            context.setParameter("");
            while (true)
            {
                //checks if the string corresponds to the TEXT command, and checks if symbols lie inside of the ASCII range (32<x<127).
                if (context.getCommand().Equals("T"))
                { 
                    if (stream.Peak() < 0) { throw new EndOfStreamException(); }

                    if ((char)stream.Peak() == ':') { break; }
                    else
                    {

                         // ASCII
                        int symbol = stream.Read();

                        context.setPacket(context.getPacket() + (char)symbol);
                        if (symbol < 32 || symbol > 127) { normalSymbolInPacket = false; break; }
                        else { context.setParameter(context.getParameter() + (char)symbol); } // creating the packet
                    }
                }

                //checks if the string corresponds to the SOUND command, and checks if symbols lie inside 0-9 and ',' (S:freq,duration).
                else if ((context.getCommand().Equals("S")))
                {
                    if (stream.Peak() < 0) { throw new EndOfStreamException(); }

                    if ((char)stream.Peak() == ':')
                        {
                             Regex rgx = new Regex(@"^[0-9]+,[0-9]+$");
                             if (rgx.IsMatch(context.getParameter()))
                             {

                             //frequency is less than 37 or more than 32767 hertz.- or -duration is less than or equal to zero.
                              String value = context.getParameter();
                              Char delimiter = ',';
                              String[] integerValue = value.Split(delimiter);
                              int frequency = Int32.Parse(integerValue[0]);
                              int duration = Int32.Parse(integerValue[1]);

                              if (frequency>=37 && frequency <= 32767 && duration>0 )
                             {
                                normalSymbolInPacket = true;
                             }
                              else { normalSymbolInPacket = false; }
                              break;
                             }
                             else
                             {
                                 normalSymbolInPacket = false;
                                 break;
                             }
                        }
                    else {
                        int symbol = stream.Read();
                        context.setPacket(context.getPacket() + (char)symbol);
                        if (symbol < 32 || symbol > 127) { normalSymbolInPacket = false; break; }
                        else { context.setParameter(context.getParameter() + (char)symbol); } // creating the parameter
                    }
                   
                }
            }
           
            return normalSymbolInPacket;
        }

        // checks the last symbol of the packet
        private static bool checkEnding(Context context, Stream stream)
        {
            char endingSymbol = 'E';
            bool engingPacket = checkedRead(endingSymbol, context, stream);
            
            return engingPacket;
        }

        // checks the symbol being Stream.Read, if it coresponds to expected symbol (exampleSymbol) returns  true.
        private static bool checkedRead(char exampleSymbol, Context context, Stream stream)
        {
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }
            context.setPacket(context.getPacket() + (char)symbol);// populate the packet

            return (char)symbol == exampleSymbol;
        }

        // executes comand 
        private static void executeCommand(Context context)
        {
            // displays parameter to console
            if (context.getCommand().Equals("T")) { Console.WriteLine("\""+context.getParameter()+"\""); }
            
            // makes the sound
            else if (context.getCommand().Equals("S"))
            {
                String value = context.getParameter();
                Char delimiter = ',';
                String[] integerValue = value.Split(delimiter);
                Console.Beep(Int32.Parse(integerValue[0]), Int32.Parse(integerValue[1]));
            }
        }
    }


}
