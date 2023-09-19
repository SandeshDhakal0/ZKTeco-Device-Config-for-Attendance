using zkemkeeper;

namespace AttendancePulller
{
    internal class Program
    {
        public Program()
        {
        }
        public enum CONSTANTS
        {
            PORT = 4370,
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Connecting...");
            CZKEM objCZKEM = new CZKEM();
            if(objCZKEM.Connect_Net("10.200.1.4", (int)CONSTANTS.PORT))
            {
                objCZKEM.SetDeviceTime2(objCZKEM.MachineNumber, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                Console.WriteLine("Connection Successful!");
                Console.WriteLine("Obtaining attendance data...");
            }
            else
            {
                Console.WriteLine("Connection Failed!");
            }
            if (objCZKEM.ReadGeneralLogData(objCZKEM.MachineNumber))
            {
                //ArrayList logs = new ArrayList();
                string log;
                string dwEnrollNumber;
                int dwVerifyMode;
                int dwInOutMode;
                int dwYear;
                int dwMonth;
                int dwDay;
                int dwHour;
                int dwMinute;
                int dwSecond;
                int dwWorkCode = 1;
                int AWorkCode;
                objCZKEM.GetWorkCode(dwWorkCode, out AWorkCode);
                //objCZKEM.SaveTheDataToFile(objCZKEM.MachineNumber, "attendance.txt", 1);
                while (true)
                {
                    if (!objCZKEM.SSR_GetGeneralLogData(
                    objCZKEM.MachineNumber,
                    out dwEnrollNumber,
                    out dwVerifyMode,
                    out dwInOutMode,
                    out dwYear,
                    out dwMonth,
                    out dwDay,
                    out dwHour,
                    out dwMinute,
                    out dwSecond,
                    ref AWorkCode
                    ))
                    {
                        break;
                    }
                    log = "User ID:" + dwEnrollNumber + " " + verificationMode(dwVerifyMode) + " " + InorOut(dwInOutMode) + " " + dwDay + "/" + dwMonth + "/" + dwYear + " " + time(dwHour) + ":" + time(dwMinute) + ":" + time(dwSecond);
                    Console.WriteLine(log);
                    //logs.Add(log);
                }
            }
            //Console.ReadLine();
        }

        static void getAttendanceLogs(CZKEM objCZKEM)
        {
            string log;
            string dwEnrollNumber;
            int dwVerifyMode;
            int dwInOutMode;
            int dwYear;
            int dwMonth;
            int dwDay;
            int dwHour;
            int dwMinute;
            int dwSecond;
            int dwWorkCode = 1;
            int AWorkCode;
            objCZKEM.GetWorkCode(dwWorkCode, out AWorkCode);
            //objCZKEM.SaveTheDataToFile(objCZKEM.MachineNumber, "attendance.txt", 1);
            while (true)
            {
                if (!objCZKEM.SSR_GetGeneralLogData(
                objCZKEM.MachineNumber,
                out dwEnrollNumber,
                out dwVerifyMode,
                out dwInOutMode,
                out dwYear,
                out dwMonth,
                out dwDay,
                out dwHour,
                out dwMinute,
                out dwSecond,
                ref AWorkCode
                ))
                {
                    break;
                }
                log = "User ID:" + dwEnrollNumber + " " + verificationMode(dwVerifyMode) + " " + InorOut(dwInOutMode) + " " + dwDay + "/" + dwMonth + "/" + dwYear + " " + time(dwHour) + ":" + time(dwMinute) + ":" + time(dwSecond);
                Console.WriteLine(log);
            }
        }

        static string time(int Time)
        {
            string stringTime = "";
            if (Time < 10)
            {
                stringTime = "0" + Time.ToString();
            }
            else
            {
                stringTime = Time.ToString();
            }
            return stringTime;
        }

        static string verificationMode(int verifyMode)
        {
            String mode = "";
            switch (verifyMode)
            {
                case 0:
                    mode = "Password";
                    break;
                case 1:
                    mode = "Fingerprint";
                    break;
                case 2:
                    mode = "Card";
                    break;
            }
            return mode;
        }

        static string InorOut(int InOut)
        {
            string InOrOut = "";
            switch (InOut)
            {
                case 0:
                    InOrOut = "IN";
                    break;
                case 1:
                    InOrOut = "OUT";
                    break;
                case 2:
                    InOrOut = "BREAK-OUT";
                    break;
                case 3:
                    InOrOut = "BREAK-IN";
                    break;
                case 4:
                    InOrOut = "OVERTIME-IN";
                    break;
                case 5:
                    InOrOut = "OVERTIME-OUT";
                    break;

            }
            return InOrOut;
        }
    }
}
