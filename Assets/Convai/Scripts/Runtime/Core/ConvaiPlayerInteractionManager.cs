using System;
using System.Collections;
using System.Linq;
using Convai.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Convai.Scripts
{
    public class ConvaiPlayerInteractionManager : MonoBehaviour
    {
        private ConvaiChatUIHandler _convaiChatUIHandler;
        private ConvaiCrosshairHandler _convaiCrosshairHandler;
        private ConvaiNPC _convaiNPC;
        private TMP_InputField _currentInputField;
        //private ConvaiInputManager _inputManager;
        private bool _stopHandlingInput;

        private void Start()
        {
            StartCoroutine(WatchForInputSubmission());
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputFieldEvents();
            StopHandlingInput();
        }

        public void Initialize(ConvaiNPC convaiNPC, ConvaiCrosshairHandler convaiCrosshairHandler, ConvaiChatUIHandler convaiChatUIHandler)
        {
            _convaiNPC = convaiNPC ? convaiNPC : throw new ArgumentNullException(nameof(convaiNPC));
            _convaiCrosshairHandler = convaiCrosshairHandler ? convaiCrosshairHandler : throw new ArgumentNullException(nameof(convaiCrosshairHandler));
            _convaiChatUIHandler = convaiChatUIHandler ? convaiChatUIHandler : throw new ArgumentNullException(nameof(convaiChatUIHandler));
            //_inputManager = ConvaiInputManager.Instance ? ConvaiInputManager.Instance : throw new InvalidOperationException("ConvaiInputManager instance not found.");
        }

        public void UpdateUserInput()
        {
            if (!UIUtilities.IsAnyInputFieldFocused()) HandleNPCInteraction();
            if (!_convaiNPC.isCharacterActive) return;

            //HandleTextInput();
            //HandleVoiceInput();
        }

        private IEnumerator WatchForInputSubmission()
        {
            while (!_stopHandlingInput)
            {
                TMP_InputField inputFieldInScene = FindActiveInputField();
                UpdateCurrentInputField(inputFieldInScene);
                yield return null;
            }
        }

        private void StopHandlingInput()
        {
            _stopHandlingInput = true;
        }

        private void UpdateCurrentInputField(TMP_InputField inputFieldInScene)
        {
            if (inputFieldInScene != null && _currentInputField != inputFieldInScene)
            {
                UnsubscribeFromInputFieldEvents();
                _currentInputField = inputFieldInScene;
                SubscribeToInputFieldEvents();
            }
        }

        private void HandleInputSubmission(string input)
        {
            if (!_convaiNPC.isCharacterActive) return;
            _convaiNPC.SendTextDataAsync(input);
            _convaiChatUIHandler.SendPlayerText(input);
            ClearInputField();
        }

        public TMP_InputField FindActiveInputField()
        {
            return _convaiChatUIHandler.GetCurrentUI().GetCanvasGroup().gameObject.GetComponentsInChildren<TMP_InputField>(true)
                .FirstOrDefault(inputField => inputField.interactable);
        }

        private void ClearInputField()
        {
            if (_currentInputField != null)
            {
                _currentInputField.text = string.Empty;
                _currentInputField.DeactivateInputField();
            }
        }

        //private void HandleTextInput()
        //{
        //    if (_currentInputField != null && _currentInputField.isFocused)
        //    {
        //        if (_inputManager.WasTextSendKeyPressed())
        //            HandleInputSubmission(_currentInputField.text);
        //        else if (_inputManager.WasCursorLockKeyPressed())
        //            ClearInputField();
        //    }
        //}

        //private void HandleVoiceInput()
        //{
        //    if (_inputManager.WasTalkKeyPressed() && !UIUtilities.IsAnyInputFieldFocused())
        //    {
        //        _convaiNPC.InterruptCharacterSpeech();
        //        UpdateActionConfig();
        //        _convaiNPC.StartListening();
        //    }
        //    else if (_inputManager.WasTalkKeyReleased() && !UIUtilities.IsAnyInputFieldFocused())
        //    {
        //        if (_convaiNPC.isCharacterActive && (_currentInputField == null || !_currentInputField.isFocused)) _convaiNPC.StopListening();
        //    }
        //}

        private void HandleNPCInteraction()
        {
            bool isNpcInConversation;
            if (TryGetComponent(out ConvaiGroupNPCController convaiGroupNPC))
                isNpcInConversation = convaiGroupNPC.IsInConversationWithAnotherNPC && ConvaiNPCManager.Instance.nearbyNPC == _convaiNPC;
            else
                isNpcInConversation = false;

            //if (isNpcInConversation && _inputManager.WasTalkKeyPressed())
            //{
            //    NPC2NPCConversationManager.Instance.EndConversation(_convaiNPC.GetComponent<ConvaiGroupNPCController>());
            //    _convaiNPC.InterruptCharacterSpeech();
            //    _convaiNPC.StartListening();
            //}
        }

        public void UpdateActionConfig()
        {
            if (_convaiNPC.ActionConfig != null && _convaiCrosshairHandler != null)
                _convaiNPC.ActionConfig.CurrentAttentionObject = _convaiCrosshairHandler.FindPlayerReferenceObject();
        }

        private void SubscribeToInputFieldEvents()
        {
            if (_currentInputField != null)
                _currentInputField.onSubmit.AddListener(HandleInputSubmission);
        }

        private void UnsubscribeFromInputFieldEvents()
        {
            if (_currentInputField != null)
                _currentInputField.onSubmit.RemoveListener(HandleInputSubmission);
        }
    }
}