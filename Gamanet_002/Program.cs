using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    }



//C:\Users\AdanaC\source\repos\Gamanet_002\Gamanet_002\bin\Debug;

    class Program
    {   
        private delegate bool Parse(MyContext context, MyStream stream);
        
        static void Main(string[] args)
        {
            String allPackets = "PT:qwerty:EPT:dsa:EPT:s:d:E";
            MyContext context = new MyContext();
            MyStream stream = new MyStream(allPackets);
            Parse[] parsers = new Parse[] { checkBegin, checkComand, checkSeparator, checkParams, checkSeparator, checkEnding};

            try
            {
                while (true)
                {
                    foreach (Parse p in parsers)
                    {
                        if (!p(context, stream)) { Console.WriteLine("NACK"); break; }
                    }
                }
            }
            catch(EndOfStreamException e)
            {
                
            }

           // Console.ReadKey();
        }

       
        private static bool checkBegin(MyContext context, MyStream stream)
        {
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }

            return (char)symbol == 'P';
        }

        private static bool checkComand(MyContext context, MyStream stream)
        {
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }

            return (char)symbol == 'T';
        }


        private static bool checkSeparator(MyContext context, MyStream stream)
        {
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }

            return (char)symbol == ':';
        }


        private static bool checkParams(MyContext context, MyStream stream)
        {
            while (true)
            {
                if (stream.Peak() < 0) { throw new EndOfStreamException(); }
                if ((char)stream.Peak() == ':') { break; }
                stream.Read();
                // ASCII
            }
            return true;

        }

        private static bool checkEnding(MyContext context, MyStream stream)
        {
            int symbol = stream.Read();

            if (symbol < 0)
            {
                throw new EndOfStreamException();
            }

            return (char)symbol == 'E';
        }
    }


}
