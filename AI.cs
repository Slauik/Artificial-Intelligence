using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    class AI
    {
        private HommSensorData sensorData;
        public Vision myVision;

        public AI(HommSensorData sensorData)
        {
            this.sensorData = sensorData;
            myVision = new Vision(sensorData.Map);
            myVision.InitMap();
        }

        private int myResourse(Resource resType)
        {
            return sensorData.MyTreasury.Where(t => t.Key.Equals(resType)).Select(t => t.Value).FirstOrDefault();
        }

        private bool CanIHireUnit(UnitType unitType, int avaliableCount) //
        {
            if (myResourse(Resource.Gold) <= 0 && avaliableCount <= 0) //
            {
                return false;
            }

            var dictionary = UnitsConstants.Current.UnitCost.Where(t=>t.Key.Equals(unitType)).Select(t=>t.Value).FirstOrDefault();

            foreach (var item in dictionary)
            {
                if (myResourse(item.Key) >= item.Value)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    



        public void Method()
        {
            Place myLocation = (Place)sensorData.Location;

            if (myVision.bottom_map[myLocation.X, myLocation.Y].dwellingIsHere != null)
            {
                if(CanIHireUnit(myVision.bottom_map[myLocation.X, myLocation.Y].
                    dwellingIsHere.UnitType, myVision.bottom_map[myLocation.X, myLocation.Y].
                    dwellingIsHere.AvailableToBuyCount))
                {
                    
                }
            }



            // Получаем путь из начальной точки в точку c координатмаи (0, 9)
            


            //var path = new[] { Direction.RightDown, Direction.RightUp, Direction.RightDown, Direction.RightUp, Direction.LeftDown, Direction.Down, Direction.RightDown, Direction.RightDown, Direction.RightUp };
            

        }
    }
}
