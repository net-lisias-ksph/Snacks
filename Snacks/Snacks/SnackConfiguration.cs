/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 * 
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    class SnackConfiguration2
    {
        private static SnackConfiguration2 snackConfig;
        int snackResourceId;

        public int SnackResourceId
        {
            get
            {
                return snackResourceId;
            }
        }

        public double SnacksPerMeal
        {
            get 
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.snacksPerMeal; 
            }
        }

        public int MealsPerDay
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.mealsPerDay;
            }
        }

        public bool loseRepWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.loseRepWhenHungry;
            }
        }

        public double repLostWhenHungry
        {
            get 
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                switch (snackProperties.repLostWhenHungry)
                {
                    case SnacksProperties.RepLoss.Low:
                    default:
                        return 0.0025;

                    case SnacksProperties.RepLoss.Medium:
                        return 0.005;

                    case SnacksProperties.RepLoss.High:
                        return 0.01;
                }
            }
        }

        public bool loseFundsWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.loseFundsWhenHuntry;
            }
        }

        public float fundsLostWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return (float)snackProperties.finePerKerbal;
            }
        }

        public bool losePartialControlWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.partialControlWhenHungry;
            }
        }

        public bool randomSnackingEnabled
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.enableRandomSnacking;
            }
        }

        public bool recyclingEnabled
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.enableRecyclers;
            }
        }
     
        private SnackConfiguration2()
        {
            PartResourceDefinition snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
            snackResourceId = snacksResource.id;
        }

        public static SnackConfiguration2 Instance()
        {
            if (snackConfig == null)
                snackConfig = new SnackConfiguration2();
            return snackConfig;
        }
    }
}
