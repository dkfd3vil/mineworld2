using System;

namespace MineWorld
{
    public class DayManager
    {
        private readonly int _lightsteps;
        private bool _goingup;
        private DateTime _lastcalclight = DateTime.Now;
        private float _light = 1f;
        private float _prevlight = 1f;

        public DayManager(int amount)
        {
            _lightsteps = amount;
        }

        public float Light
        {
            get { return _light; }
            set { _light = value; }
        }

        public void Update()
        {
            TimeSpan timespanlastcalclight = DateTime.Now - _lastcalclight;

            if (timespanlastcalclight.Seconds > _lightsteps)
            {
                if (_goingup)
                {
                    _light = _light + 0.1f;
                }
                else
                {
                    _light = _light - 0.1f;
                }

                if (_light <= 0.0f)
                {
                    _light = 0.0f;
                    _goingup = true;
                }
                if (_light >= 1.0f)
                {
                    _light = 1.0f;
                    _goingup = false;
                }
                _lastcalclight = DateTime.Now;
            }
        }

        public void SetNight()
        {
            _light = 0.0f;
        }

        public void SetDay()
        {
            _light = 1.0f;
        }
    }
}