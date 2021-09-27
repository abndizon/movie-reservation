using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation
{
    class FunctionClass
    {
        public static List<string> historyList = new List<string>();
        public void SaveXMLFile(string xml, string movie)
        {
            File.WriteAllText(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + movie + ".xml", xml);
        }
        public string ReadXMLFile(string movie)
        {
            return File.ReadAllText(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + movie + ".xml");
        }
        public bool checkFileExist(string movie)
        {
            if (File.Exists(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + movie + ".xml"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool TimeIsGreaterOrEqual(string time_one,string time_two)
        {
            TimeSpan first = TimeSpan.Parse(time_one);
            TimeSpan second = TimeSpan.Parse(time_two);

            if (first.CompareTo(second) == -1 || first.CompareTo(second) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string ConvertTime(string time)
        {
            DateTime d = DateTime.Parse(time);
            return d.ToString("HH:mm");
        }
    }
}
