using System;
using System.Globalization;
using System.Text;

namespace Terra.Core
{
    // Separate from town saves: real-world intentions never change simulation outcomes.
    [Serializable]
    public sealed class LocalActionPlan
    {
        public int version = 1;
        public int level;
        public string action = "", when = "after my next meal", savedUtc = "", checkInUtc = "";
        public bool reminder, completed;
        public bool Valid => version == 1 && level >= 0 && level < 4 && !string.IsNullOrWhiteSpace(action)
            && action.Length <= 500 && !string.IsNullOrWhiteSpace(when) && when.Length <= 200;
        public string Sentence => "When " + when + ", I will " + action + ".";
        public void SaveAt(DateTime utc, bool checkIn)
        {
            if (!Valid) throw new InvalidOperationException("Choose an action and a time first.");
            savedUtc = utc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            reminder = checkIn; completed = false;
            checkInUtc = checkIn ? utc.ToUniversalTime().AddDays(14).ToString("o", CultureInfo.InvariantCulture) : "";
        }
        public bool Due(DateTime utc) => Valid && reminder && !completed &&
            DateTime.TryParse(checkInUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var due) && utc.ToUniversalTime() >= due.ToUniversalTime();
        public string Calendar(DateTime utc)
        {
            // An all-day invitation, never an automatic calendar write or a guessed time.
            var date = utc.ToLocalTime().Date.AddDays(14);
            return "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//MyTerrain//Take it local//EN\r\nBEGIN:VEVENT\r\nUID:" +
                Guid.NewGuid().ToString("N") + "@myterrain\r\nDTSTAMP:" + utc.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'") +
                "\r\nDTSTART;VALUE=DATE:" + date.ToString("yyyyMMdd") + "\r\nDTEND;VALUE=DATE:" + date.AddDays(1).ToString("yyyyMMdd") +
                "\r\nSUMMARY:MyTerrain - check in on my plan\r\n" + Fold("DESCRIPTION:" + Escape(Sentence + " Come back to Take it local and reflect on what happened.")) +
                "\r\nEND:VEVENT\r\nEND:VCALENDAR\r\n";
        }
        static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\r", "").Replace("\n", "\\n").Replace(";", "\\;").Replace(",", "\\,");
        static string Fold(string text)
        {
            var b = new StringBuilder(); int bytes = 0;
            for(int i=0;i<text.Length;i++)
            {
                string glyph = char.IsHighSurrogate(text[i]) && i+1<text.Length && char.IsLowSurrogate(text[i+1]) ? text.Substring(i++,2) : text[i].ToString();
                int size = Encoding.UTF8.GetByteCount(glyph);
                if(bytes + size > 74){ b.Append("\r\n "); bytes = 1; }
                b.Append(glyph); bytes += size;
            }
            return b.ToString();
        }
    }
}
