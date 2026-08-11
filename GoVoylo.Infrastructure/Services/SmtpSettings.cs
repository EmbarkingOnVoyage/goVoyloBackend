using GoVoylo.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Infrastructure.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}