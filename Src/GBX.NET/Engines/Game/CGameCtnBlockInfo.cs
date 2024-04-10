﻿using GBX.NET.Components;

namespace GBX.NET.Engines.Game;

public partial class CGameCtnBlockInfo
{
    private CGameCtnBlockInfoClassic? pillar;
    private GbxRefTableFile? pillarFile;
    public CGameCtnBlockInfoClassic? Pillar { get => pillar; set => pillar = value; }

    private CGameCtnBlockUnitInfo[]? groundBlockUnitInfos;
    public CGameCtnBlockUnitInfo[]? GroundBlockUnitInfos { get => groundBlockUnitInfos; set => groundBlockUnitInfos = value; }

    private CGameCtnBlockUnitInfo[]? airBlockUnitInfos;
    public CGameCtnBlockUnitInfo[]? AirBlockUnitInfos { get => airBlockUnitInfos; set => airBlockUnitInfos = value; }

    private CSceneMobil[][]? groundMobils;
    public CSceneMobil[][]? GroundMobils { get => groundMobils; set => groundMobils = value; }

    private CSceneMobil[][]? airMobils;
    public CSceneMobil[][]? AirMobils { get => airMobils; set => airMobils = value; }

    public override IHeaderChunk? CreateHeaderChunk(uint chunkId)
    {
        if (chunkId == 0x090F4000)
        {
            var chunk = new CPlugGameSkin.HeaderChunk090F4000 { Node = new() };
            Chunks.Add(chunk);
            return chunk;
        }

        return base.CreateHeaderChunk(chunkId);
    }

    public partial class Chunk0304E005
    {
        public string? U01;
        public int U02;
        public int U03;
        public int U04;
        public int U05;
        public int U06;
        public byte U07;
        public int U08;
        public short U09;
        public short U10;

        public override void Read(CGameCtnBlockInfo n, GbxReader r)
        {
            // ChunkCrypted_Base
            U01 = r.ReadId(); // Ident.Id but why it's in CGameCtnBlockInfo?? xd
            U02 = r.ReadInt32(); // always 0?
            U03 = r.ReadInt32(); // always 0?
            U04 = r.ReadInt32(); // always 0?
            n.isPillar = r.ReadBoolean();
            U05 = r.ReadInt32(); // always 0?
            U06 = r.ReadInt32(); // always 0?
            //

            n.pillar = r.ReadNodeRef<CGameCtnBlockInfoClassic>(out n.pillarFile);

            n.groundBlockUnitInfos = r.ReadArrayNodeRef<CGameCtnBlockUnitInfo>()!;
            n.airBlockUnitInfos = r.ReadArrayNodeRef<CGameCtnBlockUnitInfo>()!;

            n.groundMobils = new CSceneMobil[r.ReadInt32()][];
            for (var i = 0; i < n.groundMobils.Length; i++)
            {
                n.groundMobils[i] = r.ReadArrayNodeRef<CSceneMobil>()!;
            }

            n.airMobils = new CSceneMobil[r.ReadInt32()][];
            for (var i = 0; i < n.airMobils.Length; i++)
            {
                n.airMobils[i] = r.ReadArrayNodeRef<CSceneMobil>()!;
            }

            U07 = r.ReadByte(); // always 0?
            U08 = r.ReadInt32(); // always 0?
            U09 = r.ReadInt16(); // always 0?
            U10 = r.ReadInt16(); // always 0?
        }
    }
}
