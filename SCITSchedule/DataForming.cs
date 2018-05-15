using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace SCITSchedule
{
    
    public static class DataForming
    {
        public static List<Appointment> List { get; set; }
        public static List<Appointment> LastList { get; set; }

        public static event EventHandler OnChanged;

        public static void FireChanged()
        {
            List<Appointment> lstDiff = new List<Appointment>();
            if (LastList != null && List != null)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    if (!LastList.Contains(List[i]))
                    {
                        lstDiff.Add(List[i]);
                    }
                }
            }
            if(lstDiff.Count > 0)
            {
                OnChanged?.Invoke(lstDiff, null);
            }
        }

        public static List<Appointment> SelectAll(String jsonString, bool isLastResult = false)
        {
            JObject d = JObject.Parse(jsonString);
            JArray list = (JArray)d["scheduleList"];
            if (isLastResult)
            {
                //LastList = new List<Appointment>(List);
                LastList = list.ToObject<List<Appointment>>();
                return LastList;
            }
            if(List != null)
            {
                LastList = new List<Appointment>(List);
            }
            List = list.ToObject<List<Appointment>>();
            List = List.FindAll(p => {
                return p.date_start > DateTime.Now;
            });
            List.Sort((Appointment a, Appointment b) =>
            {
                if(a != null && b != null && a.date_start != null && b.date_start != null)
                {
                    if(a.date_start > b.date_start)
                    {
                        return 1;
                    } else if (a.date_start < b.date_start)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            });
            FireChanged();
            System.IO.File.WriteAllText(@"lastlist.txt", jsonString);
            return List;
        }
       
    }
}
