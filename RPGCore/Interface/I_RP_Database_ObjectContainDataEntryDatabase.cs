
    using Global.ActionBus;
    using RPGCore.DataEntry;
    namespace RPGCore.Interface
    {
        public interface I_RP_Database_ObjectContainDataEntryDatabase
        {
            public string GetObjectName_ObjectContainDataEntryDatabase();
            public LocalActionBus GetRelatedActionBus();
            public Float_RPDataEntry GetFloatDataEntryByType(RP_DataEntry_EnumType type, bool allowNotExist = false);

            public Float_RPDataEntry InitializeFloatDataEntryRuntime(RP_DataEntry_EnumType type, float initValue);

        }
    }
