#region Copyright 2008, Tenaska Power Services, Co.
//
// All rights are reserved. Reproduction or transmission in whole or in 
// part, in any form or by any means, electronic, mechanical or otherwise, 
// is prohibited without the prior written consent of the copyright owner.
//
// Filename: Utilities.cs
// Created: 6/19/2008 12:26:42 PM
// Author: TPS\wallsr
//
#endregion

using System;

namespace NetworkSimulator.Bll.Utilities
{
    public static class NumberUtilities
    {
        #region Declarations

        /// <summary>
        /// This must be static so that the application does not need to instantiate a new copy of the number gen for every random number.
        /// </summary>
        private static readonly Random _randGen = new Random();

        #endregion

        #region Property Accessors

        public static Random RandomGenerator
        {
            get
            {
                return _randGen;
            }
        }

        #endregion
    }
}