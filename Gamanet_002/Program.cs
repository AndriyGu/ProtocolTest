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

    class MyStream
    {   // Опредилить конструктор который принимает параметр типа String, в котором будет содержатся буфер пакетов;
        // Определить метод Peak() который вернет текущий символ из буфера пакетов
        // Определить метод Read() который вернет текущий символ из буфера пакетов, и переведет курсор вовнутрь буфера пакетов
        String buffer;
        int position;

        public MyStream(String buffer)
        {
            this.buffer = buffer;
            this.position = 0;
        }
        public int Peak()
        {
            if (position >= buffer.Length) { return -1; }
            return buffer[position];
        }
        public int Read()
        {
            if (position >= buffer.Length) { return -1; }
            return buffer[position++];
        }
    }

    class MyContext
    {
        String command;
        String parametr;

        public void setCommand(String command) { this.command = command; }
        public String getCommand() { return this.command; }

        public void setParametr(String parametr) { this.parametr = parametr; }
        public String getParametr() { return this.parametr; }
    }



//C:\Users\AdanaC\source\repos\Gamanet_002\Gamanet_002\bin\Debug;

    class Program
    {   
        private delegate bool Parse(MyContext context, MyStream stream);
        static void Main(string[] args)
        {
            test("PT:qwerty:EPT:dsa:EPT:s:d:EPT:qwer:EPS:23123,323:E");
            
            test("PT::E");

            test("PS::E");

            Console.ReadKey();
        }


        static void test(String allPackets)
        {
            Console.WriteLine(" ========================================  Test  " + allPackets);
            MyContext context = new MyContext();
            MyStream stream = new MyStream(allPackets);
            Parse[] parsers = new Parse[] { checkBegin, checkComand, checkSeparator, checkParams, checkSeparator, checkEnding};
            
            try
            {
                while (true)
                {
                    bool ACKpacket = true;
                    foreach (Parse p in parsers)
                    {
                        if (!p(context, stream)) { ACKpacket = false; break; }
                        
                    }

                    if (ACKpacket) { Console.WriteLine("ACK "); }
                    else { Console.WriteLine("NACK"); } 
                       

                }
            }
            catch(EndOfStreamException e)
            {
                
            }

        }

       
        private static bool checkBegin(MyContext context, MyStream stream)
        {
            char beginSymbol = 'P';
            return chekingToAccord(beginSymbol, context, stream);
        }

        private static bool checkComand(MyContext context, MyStream stream)
        {
            char textCommandSymbol = 'T';
            char soundCommandSymbol = 'S';
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }

            if (((char)symbol == textCommandSymbol) || ((char)symbol == soundCommandSymbol)) { context.setCommand(((char)symbol) + ""); return true; }
            else {return false; }
        }

        private static bool checkSeparator(MyContext context, MyStream stream)
        {
            char separatorSymbol = ':';
            
            return chekingToAccord(separatorSymbol, context, stream); 
        }

        private static bool checkParams(MyContext context, MyStream stream)
        {
            bool normalSymbolInPacket = true;
            context.setParametr("");
            while (true)
            {
                if (context.getCommand().Equals("T"))
                { 
                    if (stream.Peak() < 0) { throw new EndOfStreamException(); }

                    if ((char)stream.Peak() == ':') { break; }
                    else
                    {

                    // ASCII
                    int symbol = stream.Read();

                   
                    if (symbol < 32 || symbol > 127) { normalSymbolInPacket = false; break; }
                        else { context.setParametr(context.getParametr() + (char)symbol); }
                    }
                
                }


                else if ((context.getCommand().Equals("S")))
                {
                    if (stream.Peak() < 0) { throw new EndOfStreamException(); }

                    if ((char)stream.Peak() == ':')
                        {
                             Regex rgx = new Regex(@"^[0-9]+,[0-9]+$");
                             if (rgx.IsMatch(context.getParametr()))
                             {
                               Console.WriteLine(context.getParametr() + " правдивый параметр");
                                 normalSymbolInPacket = true;
                                 break;
                             }
                             else
                             {
                                Console.WriteLine(context.getParametr() + " фиговый параметр");
                                normalSymbolInPacket = false;
                                 break;
                             }
                        }
                    else {//загнать сюда тело параметра 
                        int symbol = stream.Read();


                        if (symbol < 32 || symbol > 127) { normalSymbolInPacket = false; break; }
                        else { context.setParametr(context.getParametr() + (char)symbol); }
                    }


                    
                   
                    
                }
            }
            Console.WriteLine("Params     "+context.getParametr());
            return normalSymbolInPacket;
        }

        private static bool checkEnding(MyContext context, MyStream stream)
        {
            char endingSymbol = 'E';
            bool engingPacket = chekingToAccord(endingSymbol, context, stream);
            if (engingPacket) { Console.WriteLine("Packet is ending    " + context.getParametr()); }
            return engingPacket;
        }



        private static bool chekingToAccord(char exampleSymbol, MyContext context, MyStream stream)
        {
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }
            Console.WriteLine((char)symbol);
            return (char)symbol == exampleSymbol;
        }
    }


}
