﻿namespace BiteBoard.API.Settings
{
    public class JWTSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}