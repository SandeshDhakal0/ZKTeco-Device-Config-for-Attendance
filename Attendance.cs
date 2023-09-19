using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendancePuller
{
    public class Attendance
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        private readonly ApplicationDbContext _dbContext;

        public Attendance(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveAsync()
        {
            _dbContext.Attendances.Add(this);
            await _dbContext.SaveChangesAsync();
        }
    }
}
