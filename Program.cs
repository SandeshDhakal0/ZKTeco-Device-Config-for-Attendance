using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using zkemkeeper;

namespace AttendancePuller
{
    internal class Program
    {
        private static DbContextOptions<ApplicationDbContext> dbContextOptions;

        public Program()
        {
        }

        public enum CONSTANTS
        {
            PORT = 4370,
        }

        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .Build();
            Console.WriteLine("Connecting...");
            CZKEM objCZKEM = new CZKEM();
            if (objCZKEM.Connect_Net("10.200.1.4", (int)CONSTANTS.PORT))
            {
                objCZKEM.SetDeviceTime2(objCZKEM.MachineNumber, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                Console.WriteLine("Connection Successful!");
                Console.WriteLine("Obtaining attendance data...");

                dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                       .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .Options;

                System.Timers.Timer timer = new System.Timers.Timer
                {
                    Interval = 60000, // 1 minute
                    AutoReset = true,
                    Enabled = true
                };

                timer.Elapsed += async (sender, e) =>
                {
                    await FetchAndSaveAttendance(objCZKEM);
                };

                await FetchAndSaveAttendance(objCZKEM);

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Connection Failed!");
            }
        }

        static async Task FetchAndSaveAttendance(CZKEM objCZKEM)
        {
            List<Attendance> attendanceList = new List<Attendance>();

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

                Attendance attendance = new Attendance
                {
                    UserId = dwEnrollNumber,
                    VerifyMode = verificationMode(dwVerifyMode),
                    InOutMode = InorOut(dwInOutMode),
                    Date = new DateTime(dwYear, dwMonth, dwDay),
                    Time = new TimeSpan(dwHour, dwMinute, dwSecond)
                };

                attendanceList.Add(attendance);
            }

            if (attendanceList.Any())
            {
                using (ApplicationDbContext dbContext = new ApplicationDbContext(dbContextOptions))
                {
                    await dbContext.Attendance.AddRangeAsync(attendanceList);
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"{attendanceList.Count} attendance records saved to the database.");
                }
            }
            else
            {
                Console.WriteLine("No attendance records to save.");
            }
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

    public class Attendance
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}