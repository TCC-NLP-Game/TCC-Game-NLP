using System;
using UnityEngine;

namespace Convai.Scripts.Utils
{
    public class MicrophoneManager
    {
        /// <summary>
        ///     Singleton instance of the MicrophoneTestController.
        /// </summary>
        public static MicrophoneManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new MicrophoneManager();
                }
                return _instance;
            }
        }

        /// <summary>
        ///     Private Instance of the Singleton
        /// </summary>
        private static MicrophoneManager _instance;

        /// <summary>
        ///     Keeps track on the selected microphone device index
        /// </summary>
        private int _selectedMicrophoneIndex;

        /// <summary>
        ///     Public Getter for Selected Microphone Name
        /// </summary>
        public string SelectedMicrophoneName
        {
            get
            {
                if(_selectedMicrophoneIndex < 0 || _selectedMicrophoneIndex >= Microphone.devices.Length) return string.Empty;
                return Microphone.devices[_selectedMicrophoneIndex];
            }
        }

        /// <summary>
        /// Event indicating that the selected Microphone has changed.
        /// </summary>
        public event Action<string> OnMicrophoneDeviceChanged;

        private MicrophoneManager()
        {
            _selectedMicrophoneIndex = UISaveLoadSystem.Instance.SelectedMicrophoneDeviceNumber ;
        }

        /// <summary>
        ///     Called when the selected microphone device is changed.
        /// </summary>
        public void SetSelectedMicrophoneIndex(int selectedMicrophoneDeviceValue)
        {
            _selectedMicrophoneIndex = selectedMicrophoneDeviceValue;
            OnMicrophoneDeviceChanged?.Invoke(SelectedMicrophoneName);
        }


        /// <summary>
        ///     Returns whether any microphone is present in the system or not
        /// </summary>
        /// <returns></returns>
        public bool HasAnyMicrophoneDevices() => Microphone.devices.Length != 0;
    }
}