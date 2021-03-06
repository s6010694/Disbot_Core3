﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Disbot.Extensions
{
    public static class DiscordGameExtension
    {
        private static string[] activities = new string[] { "กำลังรดน้ำต้นไม้", "กำลังชงชา", "กำลังทำกับข้าว", "กำลังไปจ่ายตลาด", "กำลังนอน" };
        public static DiscordActivity GetRandomActivity()
        {
            var rand = new Random();
            return new DiscordActivity(activities[rand.Next(0, activities.Length)]);
        }
    }
}
