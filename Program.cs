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

                    List<string> deviceIPs = new List<string>
            {
               // "10.200.1.2",
                "10.200.1.3",
                "10.200.1.4",
                "10.200.1.5"
                //"10.10.20.102",
                //"10.10.20.104",
                //"10.10.20.105",
                //"10.10.20.103",
                //"10.10.20.115",
                //"10.10.20.106",
                //"10.10.20.207",
                //"10.10.80.2",
                //"10.10.80.4"
                // Add more device IP addresses if needed
            };

            List<Task> tasks = new List<Task>();
            foreach (string deviceIP in deviceIPs)
            {
                CZKEM objCZKEM = new CZKEM();
                if (objCZKEM.Connect_Net(deviceIP, (int)CONSTANTS.PORT))
                {
                    tasks.Add(FetchAndSaveAttendance(objCZKEM, configuration));
                }
                else
                {
                    Console.WriteLine($"Connection to device {deviceIP} failed!");
                }
            }

            // Wait for all attendance extraction tasks to complete
            await Task.WhenAll(tasks);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task FetchAndSaveAttendance(CZKEM objCZKEM, IConfiguration configuration)
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
                    Time = new TimeSpan(dwHour, dwMinute, dwSecond),
                    DeviceNumber = objCZKEM.MachineNumber
                };

                attendanceList.Add(attendance);
            }

            if (attendanceList.Any())
            {
                DbContextOptions<ApplicationDbContext> dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                    .Options;

                using (ApplicationDbContext dbContext = new ApplicationDbContext(dbContextOptions))
                {
                    await dbContext.Attendance.AddRangeAsync(attendanceList);
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"{attendanceList.Count} attendance records from device {objCZKEM.MachineNumber} saved to the database.");
                }
            }
            else
            {
                Console.WriteLine($"No attendance records from device {objCZKEM.MachineNumber} to save.");
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
        public int DeviceNumber { get; set; }
    }
}