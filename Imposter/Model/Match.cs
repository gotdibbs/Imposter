using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Imposter.Model
{
    public class Match
    {
        public Match()
        {
            TimeStamp = DateTime.Now;
        }

        public string FileName { get; set; }

        public DateTime TimeStamp { get; set; }

        public string DisplayValue
        {
            get
            {
                return string.Format("{0} {1} - {2}",
                    TimeStamp.ToShortDateString(),
                    TimeStamp.ToShortTimeString(),
                    FileName);
            }
        }
    }
}
