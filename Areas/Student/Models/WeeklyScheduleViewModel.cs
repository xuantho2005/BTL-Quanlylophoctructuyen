
using OnlineClassManagement.Models;
using System;
using System.Collections.Generic;

namespace OnlineClassManagement.Models.ViewModels
{
    public class WeeklyScheduleViewModel
    {
        public DateTime StartOfWeek { get; set; }
        public Dictionary<DayOfWeek, List<Schedule>> MorningSchedules { get; set; } = new Dictionary<DayOfWeek, List<Schedule>>();
        public Dictionary<DayOfWeek, List<Schedule>> AfternoonSchedules { get; set; } = new Dictionary<DayOfWeek, List<Schedule>>();
    }
}
