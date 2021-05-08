//-----------------------------------------------------------------------
// <copyright file="MemoryTool.cs" company="Sphere 10 Software">
//
// Copyright (c) Sphere 10 Software. All rights reserved. (http://www.sphere10.com)
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// <author>Herman Schoenfeld</author>
// <date>2018</date>
// </copyright>
//-----------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.Linq;
using Sphere10.Framework;

// ReSharper disable CheckNamespace
namespace Tools {
	public static class Memory {

        private static readonly string[] MemoryUnitStrings;

        static Memory() {
            var memUnitVals = Enum.GetValues(typeof(MemoryMetric)).Cast<int>().ToArray();
            Debug.Assert(Enumerable.Range(0, memUnitVals.Length).SequenceEqual(memUnitVals));
			MemoryUnitStrings = new string[memUnitVals.Length];
            foreach(var metric in Enum.GetValues(typeof(MemoryMetric)).Cast<MemoryMetric>()) {
                MemoryUnitStrings[(int)metric] = Tools.Enums.GetDescription(metric);
			}
		}

		public static int ToBytes(int quantityUOM, MemoryMetric UOM) => (int)Math.Round(ConvertMemoryMetric(quantityUOM, UOM, MemoryMetric.Byte), 0);

		public static string GetBytesReadable(long i) {
			var absolute_i = (i < 0 ? -i : i);
			string suffix;
			double readable;
            //if (absolute_i >= 0x1000000000000000) {
            // suffix = MemoryUnitStrings[MemoryMetric.Exabyte];
			// readable = (i >> 50);
			// readable = (readable / 1024);
            //} 

            if (absolute_i >= 0x4000000000000) {
				suffix = MemoryUnitStrings[(int)MemoryMetric.PetaByte];
				readable = (i >> 40);
				readable = (readable / 1024);
            } else if (absolute_i >= 0x10000000000) {
				suffix = MemoryUnitStrings[(int)MemoryMetric.Terrabyte];
				readable = (i >> 30);
				readable = (readable / 1024);
            } else if (absolute_i >= 0x40000000) {
				suffix = MemoryUnitStrings[(int)MemoryMetric.Gigabyte];
				readable = (i >> 20);
				readable = (readable / 1024);
            } else if (absolute_i >= 0x100000) {
				suffix = MemoryUnitStrings[(int)MemoryMetric.Megabyte];
				readable = (i >> 10);
				readable = (readable / 1024);
            } else if (absolute_i >= 0x400) {
				suffix = MemoryUnitStrings[(int)MemoryMetric.Kilobyte];
				readable = i;
				readable = (readable / 1024);
            } else {
				suffix = MemoryUnitStrings[(int)MemoryMetric.Byte];
				readable = i;
			}
			
			// Return formatted number with suffix
			return readable.ToString("0.### ") + suffix;
		}

		public static string ConvertToReadable(long quantity, MemoryMetric metric) {
            return GetBytesReadable(metric == MemoryMetric.Byte ? quantity : (long)ConvertMemoryMetric(quantity, metric, MemoryMetric.Byte));
		}


        public static double ConvertMemoryMetric(double quanity, MemoryMetric fromMetric, MemoryMetric toMetric) {
			var fromBaseMetric = ToBaseMetric(fromMetric, out var fromBaseFactor);
			var toBaseMetric = ToBaseMetric(toMetric, out var toBaseFactor);

            double conversionFactor;
            switch (fromBaseMetric) {
                case MemoryMetric.Bit:
                    switch (toBaseMetric) {
                        case MemoryMetric.Bit:
                            conversionFactor = 1;
                            break;
                        case MemoryMetric.Byte:
                            conversionFactor = 1/8D;
                            break;
                        default:
                            throw new NotSupportedException(fromBaseMetric.ToString());
                    }
                    break;

                case MemoryMetric.Byte:
                    switch (toBaseMetric) {
                        case MemoryMetric.Bit:
                            conversionFactor = 8D;
                            break;
                        case MemoryMetric.Byte:
                            conversionFactor = 1;
                            break;
                        default:
                            throw new NotSupportedException(fromBaseMetric.ToString());
                    }
                    break;
                default:
                    throw new NotSupportedException(fromBaseMetric.ToString());
            }

            return (fromBaseFactor*quanity*conversionFactor)/toBaseFactor;
        }


        public static MemoryMetric ToBaseMetric(MemoryMetric metric, out double factor) {
            switch (metric) {
                case MemoryMetric.Bit:
                    factor = 1;
                    return MemoryMetric.Bit;
                case MemoryMetric.Kilobit:
                    factor = 1E3;
                    return MemoryMetric.Bit;
                case MemoryMetric.Megabit:
                    factor = 1E6;
                    return MemoryMetric.Bit;
                case MemoryMetric.Gigabit:
                    factor = 1E9;
                    return MemoryMetric.Bit;
                case MemoryMetric.Terrabit:
                    factor = 1E12;
                    return MemoryMetric.Bit;
                case MemoryMetric.Petabit:
                    factor = 1E15D;
                    return MemoryMetric.Bit;
                case MemoryMetric.Byte:
                    factor = 1;
                    return MemoryMetric.Byte;
                case MemoryMetric.Kilobyte:
                    factor = 1E3;
                    return MemoryMetric.Byte;
                case MemoryMetric.Megabyte:
                    factor = 1E6;
                    return MemoryMetric.Byte;
                case MemoryMetric.Gigabyte:
                    factor = 1E9;
                    return MemoryMetric.Byte;
                case MemoryMetric.Terrabyte:
                    factor = 1E12;
                    return MemoryMetric.Byte;
                case MemoryMetric.PetaByte:
                    factor = 1E15;
                    return MemoryMetric.Byte;
                default:
                    throw new NotSupportedException(metric.ToString());

            }

        }
    }
}

