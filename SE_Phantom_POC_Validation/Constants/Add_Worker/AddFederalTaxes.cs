using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_Phantom_PoC_Validation.Constants
{
    public static class AddFederalTaxes
    {
        public const String TotalAllowances = "txtTotalAllowances";

        public const String AdditionalWithholding = "cboTaxOverride";

        public const String AdditionalWithholdingAmmount = "txtAdditionalAmountWithheld";

        public const String SingleRadioButton = "rdolistWithholdingStatus_0";

        public const String MarriedRadioButton = "rdolistWithholdingStatus_1";

        public const String MarriedButWithholdRadioButton = "rdolistWithholdingStatus_2";

        public const String WithHoldRadioButton = "WithHold";

        public const String Single_Value_WithHold = "Single";

        public const String Married_Value_WithHold = "Married";

        public const String MarriedBut_Value_WithHold = "Married - but withhold at higher single rate";
    }
}
