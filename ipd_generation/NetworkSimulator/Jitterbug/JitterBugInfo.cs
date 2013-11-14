#region Copyright 2009, Tenaska Power Services, Co.
//
// All rights are reserved. Reproduction or transmission in whole or in 
// part, in any form or by any means, electronic, mechanical or otherwise, 
// is prohibited without the prior written consent of the copyright owner.
//
// Filename: JitterBugInfo.cs
// Created: 4/15/2009 3:15:58 PM
// Author: TPS\wallsr
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkSimulator.Bll.Jitterbug
{
    [Serializable]
    public sealed class JitterBugInfo
    {
        #region Property Accessors

        public int TimingWindowMs { get; set; }
        public int RotateSequenceLength { get; set; }
        public decimal ShapingIncrement{ get; set; } 
        public int KMax{ get; set; }
        public int KMin{ get; set; }
        public int LMax{ get; set; }
        public int LMin{ get; set; }
        public int PenaltyForBin{ get; set; }
        public int PenaltyForDist{ get; set; } 
        public decimal MaxShapingDelaySeconds{ get; set; }
        public bool RoundIpd { get; set; }

        #endregion
    }
}
