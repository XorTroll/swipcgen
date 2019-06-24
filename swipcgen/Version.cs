using System;
using System.Collections.Generic;
using System.Text;

namespace swipcgen
{
    public enum VersionMode
    {
        Single,
        Major,
        Interval
    }

    public class Version
    {
        public byte Major { get; set; }
        public byte Minor { get; set; }
        public byte Micro { get; set; }

        public Version(string Raw)
        {
            Major = byte.Parse(Raw.Substring(0, 1));
            Minor = byte.Parse(Raw.Substring(2, 1));
            Micro = byte.Parse(Raw.Substring(4, 1));
        }
    }

    public class VersionDecorator
    {
        public string Raw { get; set; }

        public VersionDecorator(string Text)
        {
            Raw = Text;
        }

        public VersionMode Mode
        {
            get
            {
                if(Raw.EndsWith('+')) return VersionMode.Major;
                if(Raw.Contains('-')) return VersionMode.Interval;
                return VersionMode.Single;
            }
        }

        public string Version
        {
            get
            {
                return MinVersion;
            }
        }

        public string MinVersion
        {
            get
            {
                return Raw.Substring(0, 5);
            }
        }

        public string MaxVersion
        {
            get
            {
                return Raw.Substring(6, 5);
            }
        }
    }
}
