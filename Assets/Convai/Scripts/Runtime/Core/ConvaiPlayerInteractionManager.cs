using System;
using TMPro;
using UnityEngine;

namespace Convai.Scripts
{
    public class ConvaiPlayerInteractionManager : MonoBehaviour
    {
        private ConvaiNPC _convaiNPC;
        private TMP_InputField _currentInputField;


        private void OnDestroy()
        {
            UnsubscribeFromInputFieldEvents();
        }

        public void Initialize(ConvaiNPC convaiNPC)
        {
            _convaiNPC = convaiNPC ? convaiNPC : throw new ArgumentNullException(nameof(convaiNPC));
        }

        public void UpdateUserInput()
        {
            if (!_convaiNPC.isCharacterActive) return;
        }



        private void HandleInputSubmission(string input)
        {
            if (!_convaiNPC.isCharacterActive) return;
            _convaiNPC.SendTextDataAsync(input);
            ClearInputField();
        }


        private void ClearInputField()
        {
            if (_currentInputField != null)
            {
                _currentInputField.text = string.Empty;
                _currentInputField.DeactivateInputField();
            }
        }


        private void UnsubscribeFromInputFieldEvents()
        {
            if (_currentInputField != null)
                _currentInputField.onSubmit.RemoveListener(HandleInputSubmission);
        }
    }
}