using System;

namespace Learning_Management_System.Models
{
        public enum LessonStatus
        {
            Scheduled,
            Completed,
            Cancelled,
            Holiday,
        }

        public enum AttendanceStatus
        {
            Present,
            AbsentPaid,
            AbsentFree,
            Late
        }
}
