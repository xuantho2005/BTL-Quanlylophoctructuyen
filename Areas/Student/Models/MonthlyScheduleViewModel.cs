
using System;
using System.Collections.Generic;
using OnlineClassManagement.Models;

namespace OnlineClassManagement.Models.ViewModels
{
    public class MonthlyScheduleViewModel
    {
        public DateTime CurrentMonth { get; set; }
        public Dictionary<DateTime, List<Schedule>> Schedules { get; set; } = new Dictionary<DateTime, List<Schedule>>();
    }
}
