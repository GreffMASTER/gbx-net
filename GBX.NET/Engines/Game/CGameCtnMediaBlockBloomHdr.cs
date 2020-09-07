﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GBX.NET.Engines.Game
{
    [Node(0x03128000)]
    public class CGameCtnMediaBlockBloomHdr : CGameCtnMediaBlock
    {
        public Key[] Keys { get; set; }

        public CGameCtnMediaBlockBloomHdr(ILookbackable lookbackable, uint classID) : base(lookbackable, classID)
        {

        }

        [Chunk(0x03128002)]
        public class Chunk03128002 : Chunk<CGameCtnMediaBlockBloomHdr>
        {
            public override void ReadWrite(CGameCtnMediaBlockBloomHdr n, GameBoxReaderWriter rw)
            {
                n.Keys = rw.Array(n.Keys, i => new Key()
                {
                    Time = rw.Reader.ReadSingle(),
                    Intensity = rw.Reader.ReadSingle(),
                    StreaksIntensity = rw.Reader.ReadSingle(),
                    StreaksAttenuation = rw.Reader.ReadSingle()
                },
                x =>
                {
                    rw.Writer.Write(x.Time);
                    rw.Writer.Write(x.Intensity);
                    rw.Writer.Write(x.StreaksIntensity);
                    rw.Writer.Write(x.StreaksAttenuation);
                });
            }
        }

        public class Key : MediaBlockKey
        {
            public float Intensity { get; set; }
            public float StreaksIntensity { get; set; }
            public float StreaksAttenuation { get; set; }
        }
    }
}
