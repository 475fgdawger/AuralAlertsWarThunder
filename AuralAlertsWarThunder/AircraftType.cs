using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuralAlertsWarThunder
{
   public class AircraftType
    {
        public string ACtype { get; set; }
        public int AoACrit { get; set; }
        public double CritMach { get; set; }
        public int OverGHd { get; set; }
        public int GDSpd { get; set; }
        public int GuSpd  { get; set; }
        public double FuelKgPM  { get; set; }
        public int  OilCaut { get; set; }
        public int OilWarn { get; set; }
        public int H2OCaut { get; set; }
        public int H2OWarn { get; set; }

        public AircraftType()
        {

        }
         public AircraftType (string ACtype, int AOACrit, double CritMach, 
             int OverGHd, int GDSpd, int GuSpd, double FuelKgPM, int OilCaut, int OilWarn, int H2OCaut, int H2OWarn)
        {

        }
    }
}
