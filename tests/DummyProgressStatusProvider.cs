﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    public class DummyProgressStatusProvider : IProgressStatusProvider
    {
        public StringBuilder Input { get; }
        public ProgressBarType Type { get; set; }

        public DummyProgressStatusProvider()
        {
            Input = new StringBuilder();
        }

        public void Clear()
        {
            Input.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public StatusIdentifier Write(string message)
        {
            Input.Append(message);
            return StatusIdentifier.Empty;
        }

        public StatusIdentifier Write(string message, ProgressBarType type)
        {
            if (type == ProgressBarType.None)
            {
                Write(message);
                return StatusIdentifier.Empty;
            }
            Write(message + ": " + type);
            Type = type;
            return StatusIdentifier.Empty;
        }

        public void Increment(int value)
        {
            if (Type == ProgressBarType.Block)
            {
                Write(new string('+', value != -1 ? value : 10));
            }
            else
            {
                throw new NotImplementedException("The increment operation is only supported in the block progress bar.");
            }
        }
    }
}
