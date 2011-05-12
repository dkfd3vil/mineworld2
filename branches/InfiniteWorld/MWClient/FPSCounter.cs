﻿using System;
using System.Collections.Generic;
using System.Text;

namespace StateMasher
{
    class FPSCounter
    {
        private const int BUFFER_SIZE = 128;
        private int[] m_millisecs = new int[BUFFER_SIZE];
        private int m_numFrames = 0;
        private int m_first = 0;

        public FPSCounter()
        {
        }

        static private int advanceIndex(int value)
        {
            return (value + 1) % BUFFER_SIZE;
        }

        static private int advanceIndex(int startValue, int increment)
        {
            return (startValue + increment) % BUFFER_SIZE;
        }

        public void frame(int millisecs)
        {
            if (m_numFrames == BUFFER_SIZE)
            {
                m_millisecs[m_first] = millisecs;
                m_first = advanceIndex(m_first);
            }
            else
            {
                m_millisecs[advanceIndex(m_first, m_numFrames)] = millisecs;
                ++m_numFrames;
            }
        }

        public int fps()
        {
            if (m_numFrames <= 1)
            {
                return 0;
            }

            int firstFrameTime = m_millisecs[m_first];
            int lastFrameTime = m_millisecs[advanceIndex(m_first, m_numFrames - 1)];

            if (lastFrameTime == firstFrameTime)
            {
                return 0;
            }

            return m_numFrames * 1000 / (lastFrameTime - firstFrameTime);
        }
    }
}