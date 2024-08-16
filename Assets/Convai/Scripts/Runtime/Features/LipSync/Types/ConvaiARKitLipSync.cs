using System.Collections.Generic;
using System.Reflection;
using Service;


namespace Convai.Scripts.Utils.LipSync.Types
{
    public class ConvaiARKitLipSync : ConvaiVisemesLipSync
    {
        private Dictionary<string, int> _teethMapping;
        public override void Initialize(ConvaiLipSync convaiLipSync, ConvaiNPC convaiNPC)
        {
            base.Initialize(convaiLipSync, convaiNPC);
            if (HasTeethSkinnedMeshRenderer)
                _teethMapping = SetupMapping(GetTeethRegexMapping, _teethSkinMeshRenderer);
        }
        private Dictionary<string, string> GetTeethRegexMapping()
        {
            string prefix = "(?:[A-Z]\\d{1,2}_)?";
            string spacer = "[\\s_]*";
            string open = "[Oo]pen";
            return new Dictionary<string, string>()
            {
                {"KK", $"{prefix}[Jj]aw{spacer}{open}"},
                {"AA", $"{prefix}[Jj]aw{spacer}[Ff]orward"}
            };
        }
        protected override Dictionary<string, string> GetHeadRegexMapping()
        {
            string mouth = "[Mm]outh";
            string spacer = "[\\s_]*";
            string left = "[Ll]eft";
            string right = "[Rr]ight";
            string lower = "[Ll]ower";
            string upper = "[Uu]pper";
            string open = "[Oo]pen";
            string funnel = "[Ff]unnel";
            string pucker = "[Pp]ucker";
            string prefix = "(?:[A-Z]\\d{1,2}_)?";

            return new Dictionary<string, string>()
            {
                {"PP", $"{prefix}{mouth}{spacer}{pucker}"},
                {"FF", $"{prefix}{mouth}{spacer}{funnel}"},
                {"THL", $"{prefix}{mouth}{spacer}{lower}{spacer}[Dd]own{spacer}{left}"},
                {"THR", $"{prefix}{mouth}{spacer}{lower}{spacer}[Dd]own{spacer}{right}"},
                {"DDL", $"{prefix}{mouth}{spacer}[Pp]ress{spacer}{left}"},
                {"DDR", $"{prefix}{mouth}{spacer}[Pp]ress{spacer}{right}"},
                {"KK", $"{prefix}[Jj]aw{spacer}{open}"},
                {"CHL",$"{prefix}{mouth}{spacer}[Ss]tretch{spacer}{left}"},
                {"CHR",$"{prefix}{mouth}{spacer}[Ss]tretch{spacer}{right}"},
                {"SSL", $"{prefix}{mouth}{spacer}[Ss]mile{spacer}{left}"},
                {"SSR", $"{prefix}{mouth}{spacer}[Ss]mile{spacer}{right}"},
                {"NNL", $"{prefix}[Nn]ose{spacer}[Ss]neer{spacer}{left}"},
                {"NNR", $"{prefix}[Nn]ose{spacer}[Ss]neer{spacer}{right}"},
                {"RRU",$"{prefix}{mouth}{spacer}[Rr]oll{spacer}{upper}"},
                {"RRL", $"{prefix}{mouth}{spacer}[Rr]oll{spacer}{lower}"},
                {"AA", $"{prefix}[Jj]aw{spacer}{open}"},
                {"EL", $"{prefix}{mouth}{spacer}{upper}{spacer}[Uu]p{spacer}{left}"},
                {"ER", $"{prefix}{mouth}{spacer}{upper}{spacer}[Uu]p{spacer}{right}"},
                {"IHL", $"{prefix}{mouth}{spacer}[Ff]rown{spacer}{left}"},
                {"IHR",$"{prefix}{mouth}{spacer}[Ff]rown{spacer}{right}"},
                {"OU", $"{prefix}{mouth}{spacer}{pucker}"},
                {"OH", $"{prefix}{mouth}{spacer}{funnel}"},
            };
        }
        private void Update()
        {
            // Check if the dequeued frame is not null.
            if (_currentViseme == null) return;
            // Check if the frame represents silence (-2 is a placeholder for silence).
            if (_currentViseme.Sil == -2) return;

            float weight;
            List<int> knownHeadIndexs = new List<int>();
            List<int> knownTeethIndexs = new List<int>();
            foreach (PropertyInfo propertyInfo in typeof(Viseme).GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(float)) continue;
                string fieldName = propertyInfo.Name.ToUpper();
                float value = (float)propertyInfo.GetValue(_currentViseme);
                weight = fieldName switch
                {
                    "KK" => 1.0f / 1.5f,
                    "DD" => 1.0f / 0.7f,
                    "CH" => 1.0f / 2.7f,
                    "SS" => 1.0f / 1.5f,
                    "NN" => 1.0f / 2.0f,
                    "RR" => 1.0f / 0.9f,
                    "AA" => 1.0f / 2.0f,
                    "II" => 1.0f / 1.2f,
                    "OH" => 1.2f,
                    _ => 1.0f
                };

                foreach (string s in _possibleCombinations)
                {
                    float weightThisFrame = value * weight * _weightMultiplier;
                    string modifiedFieldName = fieldName + s;
                    if (HasHeadSkinnedMeshRenderer)
                    {
                        FindAndUpdateBlendWeight(_headSkinMeshRenderer, modifiedFieldName, weightThisFrame, knownHeadIndexs, _headMapping);
                    }
                    if (HasTeethSkinnedMeshRenderer)
                    {
                        FindAndUpdateBlendWeight(_teethSkinMeshRenderer, modifiedFieldName, weightThisFrame, knownTeethIndexs, _teethMapping);
                    }
                }
            }
        }
    }
}