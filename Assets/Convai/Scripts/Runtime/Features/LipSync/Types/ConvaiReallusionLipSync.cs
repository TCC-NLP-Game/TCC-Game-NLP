using System.Collections.Generic;
using System.Reflection;
using Service;
using UnityEngine;
namespace Convai.Scripts.Utils.LipSync.Types
{
    public class ConvaiReallusionLipSync : ConvaiVisemesLipSync
    {
        protected override Dictionary<string, string> GetHeadRegexMapping()
        {
            string mouth = "[Mm]outh";
            string lower = "[Ll]ower";
            string spacer = "[\\s_]*";
            string prefix = "(?:[A-Z]\\d{1,2}_)?";

            return new Dictionary<string, string>()
            {
                {"PP", $"{prefix}[Vv]{spacer}[Ee]xplosive"},
                {"FF", $"{prefix}[Vv]{spacer}[Dd]ental{spacer}[Ll]ip"},
                {"TH", $"{prefix}{mouth}{spacer}[Dd]rop{spacer}{lower}"},
                {"DDL", $"{prefix}{mouth}{spacer}[Dd]rop{spacer}[Ll]ower"},
                {"DDU", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"KKL", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Ll]ower"},
                {"KKU", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"CHL",$"{prefix}{mouth}[Dd]rop{spacer}[Ll]ower"},
                {"CHU",$"{prefix}{mouth}[Dd]rop{spacer}[Uu]pper"},
                {"CHO",$"{prefix}[Vv]{spacer}[Ll]ip{spacer}[Oo]pen"},
                {"SSL", $"{prefix}{mouth}{spacer}[Dd]rop{spacer}[Ll]ower"},
                {"SSU", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"NNL", $"{prefix}{mouth}{spacer}[Dd]rop{spacer}[Ll]ower"},
                {"NNU", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"RR",$"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"AA", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"EL", $"{prefix}{mouth}{spacer}[Dd]rop{spacer}[Ll]ower"},
                {"EU", $"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"IHL", $"{prefix}{mouth}{spacer}[Dd]rop{spacer}[Ll]ower"},
                {"IHU",$"{prefix}{mouth}{spacer}[Ss]hrug{spacer}[Uu]pper"},
                {"OH", $"{prefix}[Vv]{spacer}[Tt]ight{spacer}[Oo]"},
                {"OU", $"{prefix}[Vv]{spacer}[Tt]ight{spacer}[Oo]"},
            };
        }

        private void Update()
        {
            // Check if the dequeued frame is not null.
            if (_currentViseme == null) return;
            // Check if the frame represents silence (-2 is a placeholder for silence).
            if (_currentViseme.Sil == -2) return;

            float weight;
            List<int> knownIndexs = new List<int>();
            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f));
            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, -5.0f));
            if (HasHeadSkinnedMeshRenderer)
            {
                foreach (PropertyInfo propertyInfo in typeof(Viseme).GetProperties())
                {
                    if (propertyInfo.PropertyType != typeof(float)) continue;
                    string fieldName = propertyInfo.Name.ToUpper();
                    float value = (float)propertyInfo.GetValue(_currentViseme);
                    weight = fieldName switch
                    {
                        "TH" => 0.5f,
                        "DDL" => 0.2f / 0.7f,
                        "DDU" => 0.5f / 2.7f,
                        "KKL" => 0.5f / 1.5f,
                        "KKU" => 1.0f / 1.5f,
                        "CHL" => 0.7f / 2.7f,
                        "CHU" => 1.0f / 2.7f,
                        "CHO" => 1.0f / 2.7f,
                        "SSL" => 0.5f / 1.5f,
                        "SSU" => 1.0f / 1.5f,
                        "NNL" => 0.5f / 2.0f,
                        "NNU" => 1.0f / 2.0f,
                        "RR" => 0.5f / 0.9f,
                        "AA" => 1.0f / 2.0f,
                        "EL" => 0.7f,
                        "EU" => 0.3f,
                        "IHL" => 0.7f / 1.2f,
                        "IHU" => 0.5f / 1.2f,
                        "OH" => 1.2f,
                        _ => 1.0f
                    };
                    foreach (string s in _possibleCombinations)
                    {
                        float weightThisFrame = value * weight * _weightMultiplier;
                        string modifiedFieldName = fieldName + s;
                        FindAndUpdateBlendWeight(_headSkinMeshRenderer, modifiedFieldName, weightThisFrame, knownIndexs, _headMapping);
                    }
                }
            }


            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, -90.0f - (
                        0.2f * _currentViseme.Th
                        + 0.1f * _currentViseme.Dd
                        + 0.5f * _currentViseme.Kk
                        + 0.2f * _currentViseme.Nn
                        + 0.2f * _currentViseme.Rr
                        + 1.0f * _currentViseme.Aa
                        + 0.2f * _currentViseme.E
                        + 0.3f * _currentViseme.Ih
                        + 0.8f * _currentViseme.Oh
                        + 0.3f * _currentViseme.Ou
                    )
                    / (0.2f + 0.1f + 0.5f + 0.2f + 0.2f + 1.0f + 0.2f + 0.3f + 0.8f + 0.3f)
                    * 30f));
            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, (
                        0.1f * _currentViseme.Th
                        + 0.2f * _currentViseme.Nn
                        + 0.15f * _currentViseme.Rr
                    )
                    / (0.1f + 0.2f + 0.15f)
                    * 80f - 5f));
        }
    }
}