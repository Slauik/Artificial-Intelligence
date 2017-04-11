using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Homm.Client
{
    // Примитивы
    using DwellingEnter = HoMM.ClientClasses.Dwelling;
    using MineEnter = HoMM.ClientClasses.Mine;
    using NeutralIsHere = HoMM.ClientClasses.NeutralArmy;
    using ResourceIsHere = HoMM.ClientClasses.ResourcePile;

    // Класс TopItem
    class TopItem
    {
        // Поля-координаты
        public int X;
        public int Y;
       
        public DwellingEnter dwellingIsHere; // Вход в таверну
        public MineEnter mineIsHere; // Вход в шахту
        public NeutralIsHere neutralIsHere; // Здесь находится нейтрал 
        public ResourceIsHere resourceIsHere; // Здесь лежит ресурс

        // Конструкторы
        public TopItem(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public TopItem(int X, int Y, DwellingEnter dw) : this(X,Y)
        {
            dwellingIsHere = dw;
        }
        public TopItem(int X, int Y, MineEnter mi) : this(X, Y)
        {
            mineIsHere = mi;
        }
        public TopItem(int X, int Y, NeutralIsHere ne) : this(X, Y)
        {
            neutralIsHere = ne;
        }
        public TopItem(int X, int Y, ResourceIsHere re) : this(X, Y)
        {
            resourceIsHere = re;
        }

        // Переопределение операторов
        public static bool operator ==(TopItem left, TopItem right)
        {
            // If both are null, or both are same instance, return true. 
            if (System.Object.ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false. 
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            // Return true if the fields match: 
            return left.X == right.X && left.Y == right.Y;
        }
        public static bool operator !=(TopItem left, TopItem right)
        {
            return !(left == right);
        }

        // Явное приведение типа
        public static explicit operator TopItem(LocationInfo v)
        {
            return new TopItem(v.X, v.Y);
        }
    }
}
