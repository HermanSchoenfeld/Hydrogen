﻿using System;
using System.Collections.Generic;

namespace Sphere10.Helium.Retry
{
    public interface IRetry
    {
        void RetryCount(int totalCount, TimeSpan durationBetweenCounts);
        void RetryCount(IList<RetryCount> retryCountList);
    }
}