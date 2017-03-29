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
        private Vision myVision;

        public AI(HommSensorData sensorData)
        {
            this.sensorData = sensorData;
            myVision.InitMap();
        }

        private int myResourse(Resource resType)
        {
            return sensorData.MyTreasury.Where(t => t.Key.Equals(resType)).Select(t => t.Value).FirstOrDefault();
        }

        private bool WantToBuy(UnitType unitType, int avaliableCount) //переделать
        {
            if (myResourse(Resource.Gold) <= 0) //поменять 0 на значение из констант
            {
                return false;
            }
            if (avaliableCount > 0)
            {
                
                switch (unitType)
                {
                    case UnitType.Infantry:

                        break;
                    case UnitType.Ranged:
                        break;
                    case UnitType.Cavalry:
                        break;
                    case UnitType.Militia:
                        if (myResourse(Resource.Gold) <= avaliableCount) //сюда смотри
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        public void Method()
        {
            Place myLocation = (Place)sensorData.Location;

            if (myVision.bottom_map[myLocation.X, myLocation.Y].dwellingIsHere != null)
            {
                
            }



            // Получаем путь из начальной точки в точку c координатмаи (0, 9)
            AStarSolver pathSolver = new AStarSolver(/*sensorData.Map*/v.bottom_map);
            var path = pathSolver.GoTo(sensorData.Location, new LocationInfo(1, 4));


            //var path = new[] { Direction.RightDown, Direction.RightUp, Direction.RightDown, Direction.RightUp, Direction.LeftDown, Direction.Down, Direction.RightDown, Direction.RightDown, Direction.RightUp };
            

        }
    }
}
