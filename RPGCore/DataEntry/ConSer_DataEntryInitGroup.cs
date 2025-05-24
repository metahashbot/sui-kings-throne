using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCore.DataEntry
{
    [Serializable]
    public class  ConSer_DataEntryInitGroup
    {
        [SerializeField] 
        public List<ConSer_DataEntryInitializeConfig> InitDataEntryList;
    }
}