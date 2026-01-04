using System;

namespace Learning_Management_System.Models
{
        public enum LessonStatus
        {
            Scheduled,
            Completed,
            Canceled,
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
