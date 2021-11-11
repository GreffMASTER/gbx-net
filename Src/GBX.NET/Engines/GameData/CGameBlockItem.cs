﻿using System.Collections.Generic;

using GBX.NET.Engines.Plug;
using GBX.NET.Engines.MwFoundations;

namespace GBX.NET.Engines.GameData;

[Node(0x2E025000)]
public sealed class CGameBlockItem : CMwNod
{
    #region Fields

    private string archetypeBlockInfoId;
    private string archetypeBlockInfoCollectionId;
    private IDictionary<int, CPlugCrystal> customizedVariants;

    #endregion

    #region Properties

    [NodeMember]
    public string ArchetypeBlockInfoId
    {
        get => archetypeBlockInfoId;
        set => archetypeBlockInfoId = value;
    }

    [NodeMember]
    public string ArchetypeBlockInfoCollectionId
    {
        get => archetypeBlockInfoCollectionId;
        set => archetypeBlockInfoCollectionId = value;
    }

    [NodeMember]
    public IDictionary<int, CPlugCrystal> CustomizedVariants
    {
        get => customizedVariants;
        set => customizedVariants = value;
    }

    #endregion

    #region Constructors

    private CGameBlockItem()
    {
        archetypeBlockInfoId = null!;
        archetypeBlockInfoCollectionId = null!;
        customizedVariants = null!;
    }

    #endregion

    #region Chunks

    #region 0x000 chunk

    /// <summary>
    /// CGameBlockItem 0x000 chunk
    /// </summary>
    [Chunk(0x2E025000)]
    public class Chunk2E025000 : Chunk<CGameBlockItem>, IVersionable
    {
        private int version;

        public int Version
        {
            get => version;
            set => version = value;
        }

        public override void ReadWrite(CGameBlockItem n, GameBoxReaderWriter rw)
        {
            rw.Int32(ref version);
            rw.Id(ref n.archetypeBlockInfoId!);
            rw.Id(ref n.archetypeBlockInfoCollectionId!);
            rw.DictionaryNode(ref n.customizedVariants!);
        }
    }

    #endregion

    #endregion
}
