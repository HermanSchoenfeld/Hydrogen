﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sphere10.Framework;
using VelocityNET.Core.Configuration;
using VelocityNET.Core.Consensus;
using VelocityNET.Core.Maths;

namespace VelocityNET.Core.Mining {

	public class SingleThreadedMiner {
		protected IMiningManager _miningManager;
		private Task _miningTask;
		private CancellationTokenSource _cancelSource;
		
		public SingleThreadedMiner(string minerTag, IMiningManager miningManager) {
			_miningTask = null;
			_cancelSource = null;
			_miningManager = miningManager;
			MinerTag = minerTag;
			Status = MinerStatus.Idle;
		}

		public string MinerTag { get; }		

		public Statistics LastRate { get; set; }

		public IConfiguration Configuration { get; }

		public MinerStatus Status { get; private set; }

		public void Start() {
			Guard.Ensure(Status == MinerStatus.Idle, "Already Started");
			_cancelSource?.Cancel(false);
			_cancelSource = new CancellationTokenSource();
			Status = MinerStatus.Mining;
			_miningTask = Task.Run(Mine, _cancelSource.Token);
		}


		public void Stop() {
			Guard.Ensure(Status == MinerStatus.Mining, "Not Started");
			Status = MinerStatus.Idle;
		}

		
		protected virtual void Mine() {
			while(Status != MinerStatus.Idle) {
				var puzzle = _miningManager.RequestPuzzle(MinerTag);
				while (Status == MinerStatus.Mining && puzzle.AcceptableTimeStampRange.Start <= DateTime.UtcNow && DateTime.UtcNow <= puzzle.AcceptableTimeStampRange.End - TimeSpan.FromMilliseconds(50)) {
					unchecked { puzzle.Block.Nonce++; }
					if (puzzle.IsSolved()) {
						_miningManager.SubmitSolution(puzzle);
						break;
					}
				}
			}
		}
	}
}
