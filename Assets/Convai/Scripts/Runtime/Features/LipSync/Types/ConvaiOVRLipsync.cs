using System.Collections.Generic;
using System.Reflection;
using Service;
using UnityEngine;

namespace Convai.Scripts.Utils.LipSync.Types
{
    public class ConvaiOVRLipsync : ConvaiVisemesLipSync
    {
        private int _firstIndex;

        public override void Initialize(ConvaiLipSync convaiLipSync, ConvaiNPC convaiNPC)
        {
            base.Initialize(convaiLipSync, convaiNPC);
            _firstIndex = convaiLipSync.firstIndex;
        }

        protected override Dictionary<string, string> GetHeadRegexMapping()
        {
            const string mouth = "[Mm]outh";
            const string spacer = "[\\s_]*";
            const string left = "[Ll]eft";
            const string right = "[Rr]ight";
            const string lower = "[Ll]ower";
            const string upper = "[Uu]pper";
            const string open = "[Oo]pen";
            const string funnel = "[Ff]unnel";
            const string pucker = "[Pp]ucker";
            const string prefix = "(?:[A-Z]\\d{1,2}_)?";

            return new Dictionary<string, string>
            {
                {"PP", $"{prefix}{mouth}{spacer}{pucker}"},
                {"FF", $"{prefix}{mouth}{spacer}{funnel}"},
                {"THL", $"{prefix}{mouth}{spacer}{lower}{spacer}[Dd]own{spacer}{left}"},
                {"THR", $"{prefix}{mouth}{spacer}{lower}{spacer}[Dd]own{spacer}{right}"},
                {"DDL", $"{prefix}{mouth}{spacer}[Pp]ress{spacer}{left}"},
                {"DDR", $"{prefix}{mouth}{spacer}[Pp]ress{spacer}{right}"},
                {"KK", $"{prefix}[Jj]aw{spacer}{open}"},
                {"CHL", $"{prefix}{mouth}{spacer}[Ss]tretch{spacer}{left}"},
                {"CHR", $"{prefix}{mouth}{spacer}[Ss]tretch{spacer}{right}"},
                {"SSL", $"{prefix}{mouth}{spacer}[Ss]mile{spacer}{left}"},
                {"SSR", $"{prefix}{mouth}{spacer}[Ss]mile{spacer}{right}"},
                {"NNL", $"{prefix}[Nn]ose{spacer}[Ss]neer{spacer}{left}"},
                {"NNR", $"{prefix}[Nn]ose{spacer}[Ss]neer{spacer}{right}"},
                {"RRU", $"{prefix}{mouth}{spacer}[Rr]oll{spacer}{upper}"},
                {"RRL", $"{prefix}{mouth}{spacer}[Rr]oll{spacer}{lower}"},
                {"AA", $"{prefix}[Jj]aw{spacer}[Oo]pen"},
                {"EL", $"{prefix}{mouth}{spacer}{upper}{spacer}[Uu]p{spacer}{left}"},
                {"ER", $"{prefix}{mouth}{spacer}{upper}{spacer}[Uu]p{spacer}{right}"},
                {"IHL", $"{prefix}{mouth}{spacer}[Ff]rown{spacer}{left}"},
                {"IHR", $"{prefix}{mouth}{spacer}[Ff]rown{spacer}{right}"},
                {"OU", $"{prefix}{mouth}{spacer}{pucker}"},
                {"OH", $"{prefix}{mouth}{spacer}{funnel}"},
            };
        }

        private void Update()
        {
            if (_currentViseme == null || _currentViseme.Sil == -2) return;

            float weight;
            List<int> knownIndexes = new List<int>();

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
                        FindAndUpdateBlendWeight(_headSkinMeshRenderer, modifiedFieldName, weightThisFrame, knownIndexes, _headMapping);
                    }
                }
            }

            UpdateJawBoneRotation(new Vector3(0.0f, 0.0f, CalculateJawRotation()));
            UpdateTongueBoneRotation(new Vector3(0.0f, 0.0f, CalculateTongueRotation()));

            if (_teethSkinMeshRenderer.sharedMesh.blendShapeCount < (_firstIndex + 15)) return;

            for (int i = 0; i < 15; i++)
            {
                float visemeValue = GetVisemeValueByIndex(i);
                _teethSkinMeshRenderer.SetBlendShapeWeightInterpolate(_firstIndex + i, visemeValue * _weightMultiplier, Time.deltaTime);
            }
        }

        private float CalculateJawRotation()
        {
            float totalWeight = 0.2f + 0.1f + 0.5f + 0.2f + 0.2f + 1.0f + 0.2f + 0.3f + 0.8f + 0.3f;
            float rotation = (0.2f * _currentViseme.Th
                             + 0.1f * _currentViseme.Dd
                             + 0.5f * _currentViseme.Kk
                             + 0.2f * _currentViseme.Nn
                             + 0.2f * _currentViseme.Rr
                             + 1.0f * _currentViseme.Aa
                             + 0.2f * _currentViseme.E
                             + 0.3f * _currentViseme.Ih
                             + 0.8f * _currentViseme.Oh
                             + 0.3f * _currentViseme.Ou) / totalWeight;

            return -90.0f - rotation * 30f;
        }

        private float CalculateTongueRotation()
        {
            float totalWeight = 0.1f + 0.2f + 0.15f;
            float rotation = (0.1f * _currentViseme.Th
                             + 0.2f * _currentViseme.Nn
                             + 0.15f * _currentViseme.Rr) / totalWeight;

            return rotation * 80f - 5f;
        }

        private float GetVisemeValueByIndex(int index)
        {
            return index switch
            {
                0 => _currentViseme.Sil,
                1 => _currentViseme.Pp,
                2 => _currentViseme.Ff,
                3 => _currentViseme.Th,
                4 => _currentViseme.Dd,
                5 => _currentViseme.Kk,
                6 => _currentViseme.Ch,
                7 => _currentViseme.Ss,
                8 => _currentViseme.Nn,
                9 => _currentViseme.Rr,
                10 => _currentViseme.Aa,
                11 => _currentViseme.E,
                12 => _currentViseme.Ih,
                13 => _currentViseme.Oh,
                14 => _currentViseme.Ou,
                _ => 0.0f
            };
        }
    }
}