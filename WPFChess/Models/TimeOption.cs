using System;

namespace ChessWPF.Models
{
    public class TimeOption
    {
        public TimeSpan Time { get; set; }
        public string Display { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj is TimeOption other)
            {
                return Time == other.Time;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }
    }
}
