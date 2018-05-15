using System;
using System.Collections.Generic;
using System.Text;

namespace SCITSchedule
{
    public class Appointment
    {
        public DateTime? date_end { get; set; }
        public DateTime? date_start { get; set; }
        public object endDate { get; set; }
        public object endTime { get; set; }
        public string indate { get; set; }
        public string ldate { get; set; }
        public string lmember_id { get; set; }
        public string member_id { get; set; }
        public string schedule_color { get; set; }
        public string schedule_content { get; set; }
        public string schedule_seq { get; set; }
        public string schedule_title { get; set; }
        public object startDate { get; set; }
        public object startTime { get; set; }
        public bool Highlight { get; set; }
        public int RowNum { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Appointment an = ((Appointment)obj);
            if(an.date_start == this.date_start
                && an.schedule_title == schedule_title
                && an.schedule_content == schedule_content)
            {
                return true;
            }
            
            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode() => (date_start?.GetHashCode() ?? 1) ^ (schedule_title?.GetHashCode() ?? 1) ^ (schedule_content?.GetHashCode() ?? 1);
    }
}
