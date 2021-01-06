﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sphere10.Framework;
using VelocityNET.Core.Configuration;

namespace VelocityNET.Core.Maths {

    public class ASERT_RTT : IDAAlgorithm {

		public ASERT_RTT(ITargetAlgorithm targetAlgorithm, ASERTConfiguration configuration) {
            PoWAlgorithm = targetAlgorithm;
            Config = configuration;
		}

        protected ITargetAlgorithm PoWAlgorithm { get; }

		protected ASERTConfiguration Config { get; }

        public bool RealTime => true;

		public virtual uint CalculateNextBlockTarget(IEnumerable<DateTime> previousBlockTimestamps, uint previousCompactTarget, uint blockNumber) {
            if (!previousBlockTimestamps.Any())
                return PoWAlgorithm.MinCompactTarget; // start at minimum

            var lastBlockTime = previousBlockTimestamps.First();
            return CalculateNextBlockTarget(
               previousCompactTarget,
               (int)DateTime.UtcNow.Subtract(lastBlockTime).TotalSeconds,
               (int)Config.BlockTime.TotalSeconds,
               (int)Config.RelaxationTime.TotalSeconds
            );
        }

        public uint CalculateNextBlockTarget (uint previousCompactTarget, int timestampDelta, int blockTimeSec, int relaxationTime) {
            const int FloatingPointResolution = 6;
            var prevBlockTarget = PoWAlgorithm.ToTarget(previousCompactTarget);
            var exp = FixedPoint.Exp((timestampDelta - blockTimeSec) / (FixedPoint)relaxationTime);
            var expNumerator = new BigInteger(exp * FixedPoint.Pow(10, FloatingPointResolution));
            var expDenominator = new BigInteger(Math.Pow(10.0D, FloatingPointResolution));
            var nextTarget = prevBlockTarget * expNumerator / expDenominator;
            var nextCompactTarget = PoWAlgorithm.FromTarget(nextTarget);
            return nextCompactTarget;
        }




	}


}