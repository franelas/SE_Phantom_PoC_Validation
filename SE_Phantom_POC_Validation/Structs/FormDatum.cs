using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_Phantom_PoC_Validation.Structs
{
    public struct FormDatum
    {
        public String FieldId;

        public String FieldValue;

        public int fieldType;   

        public FormDatum(string fieldValue, string fieldId, int fieldType) : this()
        {
            this.FieldId = fieldId;
            this.FieldValue = fieldValue;
            this.fieldType = fieldType;
        }
    }
}
