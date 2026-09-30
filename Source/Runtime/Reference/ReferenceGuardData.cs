using System;
using System.Collections.Generic;

namespace BadPractice.Cursoriam.Inspector
{
    [Serializable]
    public sealed class ReferenceGuardData
    {
        public bool IsAutoBackupActive = true;

        public bool IsAutoRestoreActive;

        public bool IsCleared;

        public Dictionary<string, object> SavedFields = new();
    }
}