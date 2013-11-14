#region Copyright 2009, Tenaska Power Services, Co.
//
// All rights are reserved. Reproduction or transmission in whole or in 
// part, in any form or by any means, electronic, mechanical or otherwise, 
// is prohibited without the prior written consent of the copyright owner.
//
// Filename: CorrelatedSet.cs
// Created: 4/15/2009 12:30:27 PM
// Author: TPS\wallsr
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NetworkSimulator.Bll.Jitterbug;
using NetworkSimulator.Bll.Utilities;

namespace NetworkSimulator.Bll.RunSets
{
    public sealed class CorrelatedSampleSet : SetBase
    {
        #region Declarations



        #endregion

        #region Instantiation & Setup

        public CorrelatedSampleSet(SetInfo setInfo): base(setInfo){}

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region System.Object Methods

        #endregion

        #region Operator Overrides

        #endregion

        #region Property Accessors

        #endregion

        protected override List<decimal> SampleIpds()
        {
            List<decimal> sampledIpds = new List<decimal>(_setInfo.SetSize);

            while (sampledIpds.Count < _setInfo.SetSize)
            {
                int index = NumberUtilities.RandomGenerator.Next(0, _masterSampleIpds.Count);

                for (int i = 0; i < _setInfo.SampleSize; i++)
                {
                    sampledIpds.Add(_masterSampleIpds[index % _masterSampleIpds.Count]);
                    index++;
                }
            }

            return sampledIpds;
        }
    }
}
