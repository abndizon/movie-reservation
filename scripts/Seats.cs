using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation
{
    public class Seats
    {
        private List<string> reservedSeats;
        private List<string> usedSeats;

        public List<string> _reservedSeats
        {
            set { reservedSeats = value; }
            get { return reservedSeats; }
        }
        public List<string> _usedSeats
        {
            set { usedSeats = value; }
            get { return usedSeats; }
        }
    }
}
