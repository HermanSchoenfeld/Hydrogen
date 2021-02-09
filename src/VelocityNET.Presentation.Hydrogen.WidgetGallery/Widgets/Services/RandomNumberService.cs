﻿using System;

namespace VelocityNET.Presentation.Hydrogen.WidgetGallery.Widgets.Services {

    public class RandomNumberService : IRandomNumberService {
        public int GetRandomNumber() => Random.Next();

        private Random Random { get; } = new();
    }

    public interface IRandomNumberService {
        int GetRandomNumber();
    }
}