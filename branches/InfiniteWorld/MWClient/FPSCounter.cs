namespace MineWorld
{
    internal class FpsCounter
    {
        private const int BufferSize = 128;
        private readonly int[] _mMillisecs = new int[BufferSize];
        private int _mFirst;
        private int _mNumFrames;

        private static int AdvanceIndex(int value)
        {
            return (value + 1)%BufferSize;
        }

        private static int AdvanceIndex(int startValue, int increment)
        {
            return (startValue + increment)%BufferSize;
        }

        public void Frame(int millisecs)
        {
            if (_mNumFrames == BufferSize)
            {
                _mMillisecs[_mFirst] = millisecs;
                _mFirst = AdvanceIndex(_mFirst);
            }
            else
            {
                _mMillisecs[AdvanceIndex(_mFirst, _mNumFrames)] = millisecs;
                ++_mNumFrames;
            }
        }

        public int Fps()
        {
            if (_mNumFrames <= 1)
            {
                return 0;
            }

            int firstFrameTime = _mMillisecs[_mFirst];
            int lastFrameTime = _mMillisecs[AdvanceIndex(_mFirst, _mNumFrames - 1)];

            if (lastFrameTime == firstFrameTime)
            {
                return 0;
            }

            return _mNumFrames*1000/(lastFrameTime - firstFrameTime);
        }
    }
}