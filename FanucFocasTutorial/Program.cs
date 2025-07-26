using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FanucFocasTutorial1
{
    class Program
    {
        static ushort _handle = 0;
        static short _ret = 0;

        static bool _exit = false;

        static void Main(string[] args)
        {

            //Thread t = new Thread(new ThreadStart(ExitCheck));
            //t.Start();

            _ret = Focas1.cnc_allclibhndl3("192.168.101.170", 8193, 6, out _handle);

            if (_ret != Focas1.EW_OK)
            {
                Console.WriteLine($"Unable to connect to 192.168.101.170 on port 8193\n\nReturn Code: {_ret}\n\nExiting....");
                Console.Read();
            }
            else
            {
                Console.WriteLine($"переменная _handle = {_handle}");

                string mode = GetMode();
                Console.WriteLine($"\n\nРежим: {mode}");

                string status = GetStatus();
                Console.WriteLine($"\n\nСтатус включения: {status}\n\n");

                GetMacro(801);  //устанавливаем переменную #801 у станка
                GetMacro(802);  //устанавливаем переменную #802 у станка
                GetMacro(803);  //устанавливаем переменную #803 у станка
                GetMacro(804);  //устанавливаем переменную #804 у станка
                Console.WriteLine("");
                SetMacro(813,1,0); //устанавливаем переменную #813 у станка
                GetMacro(813); //читаем переменную #814 у станка
                Console.WriteLine("");
                //DNCStart(); // вызываем программу через DNC

                while (!_exit)
                {
                    bool isRunning = GetOpSignal();
                    Console.Write($"\rТекущее состояние станка: {(isRunning ? "Программа запущена и работает" : "Программа не запущена")}\r");
                    Thread.Sleep(500);
                }




            }
        }

        //private static void ExitCheck()
        //{
        //    while (Console.ReadLine() != "exit")
        //    {
        //        continue;
        //    }

        //    _exit = true;
        //}


        public static bool GetOpSignal()
        {
            if (_handle == 0)
            {
                Console.WriteLine("Error: Please obtain a handle before calling this method");
                return false;
            }

            short addr_kind = 1; // F
            short data_type = 0; // Byte
            ushort start = 0;
            ushort end = 0;
            ushort data_length = 9; // 8 + N
            Focas1.IODBPMC0 pmc = new Focas1.IODBPMC0();

            _ret = Focas1.pmc_rdpmcrng(_handle, addr_kind, data_type, start, end, data_length, pmc);
            // о pmc_rdpmcrng читать также тут https://docs.aveva.com/bundle/pi-connector-for-fanuc-focas/page/1010302.html
            if (_ret != Focas1.EW_OK)
            {
                Console.WriteLine($"Error: Unable to ontain the OP Signal");
                return false;
            }

            return pmc.cdata[0].GetBit(7);

        }

        public static string GetMode()
        {
            if (_handle == 0)
            {
                Console.WriteLine("Error: Please obtain a handle before calling this method");
                return "";
            }

            Focas1.ODBST Mode = new Focas1.ODBST();

            _ret = Focas1.cnc_statinfo(_handle, Mode);

            if (_ret != 0)
            {
                Console.WriteLine($"Error: Unable to obtain mode.\nReturn Code: {_ret}");
                return "";
            }

            string modestr = ModeNumberToString(Mode.aut);

            return $"{modestr}";
        }

        public static string ModeNumberToString(int num)
        {
            switch (num)
            {
                case 0: { return "MDI"; }
                case 1: { return "MEM"; }
                case 3: { return "EDIT"; }
                case 4: { return "HND"; }
                case 5: { return "JOG"; }
                case 6: { return "Teach in JOG"; }
                case 7: { return "Teach in HND"; }
                case 8: { return "INC"; }
                case 9: { return "ZRN"; }
                case 10: { return "DNC"; }
                default: { return "UNAVAILABLE"; }
            }
        }

        public static string GetStatus()
        {
            if (_handle == 0)
            {
                Console.WriteLine("Ошибка: получите handle перед вызовом этого метода");
                return "";
            }

            Focas1.ODBST Status = new Focas1.ODBST();

            _ret = Focas1.cnc_statinfo(_handle, Status);

            if (_ret != 0)
            {
                Console.WriteLine($"Error: Unable to obtain status.\nReturn Code: {_ret}");
                return "";
            }

            string statusstr = StatusNumberToString(Status.run);

            return $"{statusstr}";
        }

        public static string StatusNumberToString(int num)
        {
            switch (num)
            {
                case 0: { return "В ожидании"; }
                case 1: { return "STOP"; }
                case 2: { return "HOLD"; }
                case 3: { return "START"; }
                case 4: { return "MSTR"; }
                default: { return "UNAVAILABLE"; }
            }
        }
        //public static string DNCStart()
        //{
        //    Focas1.cnc_dncstart(_handle);
        //    Focas1.cnc_dnc(_handle,"\n", 1);
        //    Focas1.cnc_dnc(_handle, "M3S2000\n", 8);
        //    Focas1.cnc_dnc(_handle, "G04P999999\n", 11);
        //    Focas1.cnc_dnc(_handle, "%", 1);

        //    return ("");
        //}


        public static string GetMacro(short number)
        {
            
            Focas1.ODBM macro = new Focas1.ODBM();
            string strVal;
            _ret = Focas1.cnc_rdmacro(_handle, number, 10, macro);
                strVal = string.Format("{0:d9}", Math.Abs(macro.mcr_val));
                if (0 < macro.dec_val) strVal = strVal.Insert(9 - macro.dec_val, ".");
                if (macro.mcr_val < 0) strVal = "-" + strVal;
                Console.WriteLine("Переменная {0} = {1}", number, strVal);
            return ("");
        }

        public static string SetMacro(short number, int value, short dec)
        {
            Focas1.cnc_wrmacro(_handle, number, 10, value, dec);
            //_ret = Focas1.cnc_wrmacro(_handle, number, 10, value, dec);
            return ("");
        }
    }
}
