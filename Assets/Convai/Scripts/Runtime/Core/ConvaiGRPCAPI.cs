using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convai.Scripts.Runtime.Utils;
using Convai.Scripts.Utils.LipSync;
using Google.Protobuf;
using Grpc.Core;
using Service;
using UnityEngine;
using static Service.GetResponseRequest.Types;

namespace Convai.Scripts.Utils
{
    public class WavHeaderParser
    {
        public WavHeaderParser(byte[] wavBytes)
        {
            // Ensure the byte array is not null and has enough bytes to contain a header
            if (wavBytes == null || wavBytes.Length < 44)
                throw new ArgumentException("Invalid WAV byte array.");

            // Parse the number of channels (2 bytes at offset 22)
            NumChannels = BitConverter.ToInt16(wavBytes, 22);

            // Parse the sample rate (4 bytes at offset 24)
            SampleRate = BitConverter.ToInt32(wavBytes, 24);

            // Parse the bits per sample (2 bytes at offset 34)
            BitsPerSample = BitConverter.ToInt16(wavBytes, 34);

            // Parse the Subchunk2 size (data size) to help calculate the data length
            DataSize = BitConverter.ToInt32(wavBytes, 40);
        }

        public int SampleRate { get; }
        public int NumChannels { get; }
        public int BitsPerSample { get; }
        public int DataSize { get; }

        public float CalculateDurationSeconds()
        {
            // Calculate the total number of samples in the data chunk
            int totalSamples = DataSize / (NumChannels * (BitsPerSample / 8));

            // Calculate the duration in seconds
            return (float)totalSamples / SampleRate;
        }
    }

    /// <summary>
    ///     This class is dedicated to manage all communications between the Convai server and plugin, in addition to
    ///     processing any data transmitted during these interactions. It abstracts the underlying complexities of the plugin,
    ///     providing a seamless interface for users. Modifications to this class are discouraged as they may impact the
    ///     stability and functionality of the system. This class is maintained by the development team to ensure compatibility
    ///     and performance.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ConvaiNPCManager))]
    [AddComponentMenu("Convai/Convai GRPC API")]
    [HelpURL(
        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/scripts-overview/convaigrpcapi.cs")]
    public class ConvaiGRPCAPI : MonoBehaviour
    {
        public static ConvaiGRPCAPI Instance;
        private readonly List<string> _stringUserText = new();
        private ConvaiNPC _activeConvaiNPC;

        private string _apiKey;
        private CancellationTokenSource _cancellationTokenSource;
        //private ConvaiChatUIHandler _chatUIHandler;

        private void Awake()
        {
            // Singleton pattern: Ensure only one instance of this script is active.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Load API key from a ScriptableObject in Resources folder.
            ConvaiAPIKeySetup.GetAPIKey(out _apiKey);

            // Find and store a reference to the ConvaiChatUIHandler component in the scene.
            //_chatUIHandler = FindObjectOfType<ConvaiChatUIHandler>();
        }

        private void Start()
        {
            ConvaiNPCManager.Instance.OnActiveNPCChanged += HandleActiveNPCChanged;
            _cancellationTokenSource = new CancellationTokenSource();
            MainThreadDispatcher.CreateInstance();
        }

        //private void FixedUpdate()
        //{
        //    // Check if there are pending user texts to display
        //    // If chatUIHandler is available, send the first user text in the list
        //    if (_stringUserText.Count > 0 && _chatUIHandler != null)
        //    {
        //        _chatUIHandler.SendPlayerText(_stringUserText[0]);
        //        // Remove the displayed user text from the list
        //        _stringUserText.RemoveAt(0);
        //    }
        //}

        private void OnDestroy()
        {
            ConvaiNPCManager.Instance.OnActiveNPCChanged -= HandleActiveNPCChanged;

            InterruptCharacterSpeech(_activeConvaiNPC);
            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                // Handle the Exception, which can occur if the CancellationTokenSource is already disposed. 
                Logger.Warn("Exception in OnDestroy: " + ex.Message, Logger.LogCategory.Character);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }


        /// <summary>
        ///     Asynchronously initializes a session ID by communicating with a gRPC service and returns the session ID if
        ///     successful.
        /// </summary>
        /// <param name="characterName">The name of the character for which the session is being initialized.</param>
        /// <param name="client">The gRPC service client used to make the call to the server.</param>
        /// <param name="characterID">The unique identifier for the character.</param>
        /// <param name="sessionID">The session ID that may be updated during the initialization process.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the initialized session ID if
        ///     successful, or null if the initialization fails.
        /// </returns>
        public static async Task<string> InitializeSessionIDAsync(string characterName, ConvaiService.ConvaiServiceClient client, string characterID, string sessionID)
        {
            Logger.DebugLog("Initializing SessionID for character: " + characterName, Logger.LogCategory.Character);

            if (client == null)
            {
                Logger.Error("gRPC client is not initialized.", Logger.LogCategory.Character);
                return null;
            }

            using AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call = client.GetResponse();
            GetResponseRequest getResponseConfigRequest = new()
            {
                GetResponseConfig = new GetResponseConfig
                {
                    CharacterId = characterID,
                    ApiKey = Instance._apiKey,
                    SessionId = sessionID,
                    AudioConfig = new AudioConfig { DisableAudio = true }
                }
            };

            try
            {
                await call.RequestStream.WriteAsync(getResponseConfigRequest);
                await call.RequestStream.WriteAsync(new GetResponseRequest
                {
                    GetResponseData = new GetResponseData
                    {
                        TextData = "Repeat the following exactly as it is: [Hii]"
                    }
                });

                await call.RequestStream.CompleteAsync();

                while (await call.ResponseStream.MoveNext())
                {
                    GetResponseResponse result = call.ResponseStream.Current;

                    if (!string.IsNullOrEmpty(result.SessionId))
                    {
                        Logger.DebugLog("SessionID Initialization SUCCESS for: " + characterName,
                            Logger.LogCategory.Character);
                        sessionID = result.SessionId;
                        return sessionID;
                    }
                }

                Logger.Exception("SessionID Initialization FAILED for: " + characterName, Logger.LogCategory.Character);
            }
            catch (RpcException rpcException)
            {
                switch (rpcException.StatusCode)
                {
                    case StatusCode.Cancelled:
                        Logger.Exception(rpcException, Logger.LogCategory.Character);
                        break;
                    case StatusCode.Unknown:
                        Logger.Error($"Unknown error from server: {rpcException.Status.Detail}",
                            Logger.LogCategory.Character);
                        break;
                    default:
                        throw;
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, Logger.LogCategory.Character);
            }

            return null;
        }


        /// <summary>
        ///     Sends text data to the server and processes the response.
        /// </summary>
        /// <param name="client">The gRPC client used to communicate with the server.</param>
        /// <param name="userText">The text data to send to the server.</param>
        /// <param name="characterID">The ID of the character that is sending the text.</param>
        /// <param name="isActionActive">Indicates whether actions are active.</param>
        /// <param name="isLipSyncActive">Indicates whether lip sync is active.</param>
        /// <param name="actionConfig">The action configuration.</param>
        /// <param name="faceModel">The face model.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendTextData(ConvaiService.ConvaiServiceClient client, string userText, string characterID, bool isActionActive, bool isLipSyncActive,
            ActionConfig actionConfig, FaceModel faceModel)
        {
            Debug.Log("Client: " + client);
            AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call =
                GetAsyncDuplexStreamingCallOptions(client);
            Debug.Log("Client later: " + client);

            GetResponseRequest getResponseConfigRequest = CreateGetResponseRequest(
                isActionActive,
                isLipSyncActive,
                0,
                characterID,
                actionConfig,
                faceModel);

            try
            {
                await call.RequestStream.WriteAsync(getResponseConfigRequest);
                await call.RequestStream.WriteAsync(new GetResponseRequest
                {
                    GetResponseData = new GetResponseData
                    {
                        TextData = userText
                    }
                });
                await call.RequestStream.CompleteAsync();

                // Store the task that receives results from the server.
                Task receiveResultsTask = Task.Run(
                    async () => { await ReceiveResultFromServer(call, _cancellationTokenSource.Token); },
                    _cancellationTokenSource.Token);

                // Await the task if needed to ensure it completes before this method returns [OPTIONAL]
                await receiveResultsTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, Logger.LogCategory.Character);
            }
        }

        // This method will be called whenever the active NPC changes.
        private void HandleActiveNPCChanged(ConvaiNPC newActiveNPC)
        {
            if (newActiveNPC != null)
                InterruptCharacterSpeech(newActiveNPC);

            // Cancel the ongoing gRPC call
            try
            {
                _cancellationTokenSource?.Cancel();
            }
            catch (Exception e)
            {
                // Handle the Exception, which can occur if the CancellationTokenSource is already disposed. 
                Logger.Warn("Exception in GRPCAPI:HandleActiveNPCChanged: " + e.Message,
                    Logger.LogCategory.Character);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                Logger.Info("The Cancellation Token Source was Disposed in GRPCAPI:HandleActiveNPCChanged",
                    Logger.LogCategory.Character);
            }

            _cancellationTokenSource = new CancellationTokenSource(); // Create a new token for future calls
            _activeConvaiNPC = newActiveNPC;
        }

        /// <summary>
        ///     Starts recording audio and sends it to the server for processing.
        /// </summary>
        /// <param name="client">gRPC service Client object</param>
        /// <param name="isActionActive">Bool specifying whether we are expecting action responses</param>
        /// <param name="isLipSyncActive"></param>
        /// <param name="recordingFrequency">Frequency of the audio being sent</param>
        /// <param name="recordingLength">Length of the recording from the microphone</param>
        /// <param name="characterID">Character ID obtained from the playground</param>
        /// <param name="actionConfig">Object containing the action configuration</param>
        /// <param name="faceModel"></param>
        public async Task StartRecordAudio(ConvaiService.ConvaiServiceClient client, bool isActionActive, bool isLipSyncActive, int recordingFrequency, int recordingLength,
            string characterID, ActionConfig actionConfig, FaceModel faceModel)
        {
            AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call = GetAsyncDuplexStreamingCallOptions(client);

            GetResponseRequest getResponseConfigRequest = CreateGetResponseRequest(isActionActive, isLipSyncActive, recordingFrequency, characterID, actionConfig, faceModel);

            Logger.DebugLog(getResponseConfigRequest.ToString(), Logger.LogCategory.Character);

            try
            {
                await call.RequestStream.WriteAsync(getResponseConfigRequest);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, Logger.LogCategory.Character);
                return; // early return on error
            }

            AudioClip audioClip = Microphone.Start(MicrophoneManager.Instance.SelectedMicrophoneName, false, recordingLength, recordingFrequency);

            MicrophoneTestController.Instance.CheckMicrophoneDeviceWorkingStatus(audioClip);

            Logger.Info(_activeConvaiNPC.characterName + " is now listening", Logger.LogCategory.Character);
            OnPlayerSpeakingChanged?.Invoke(true);

            await ProcessAudioContinuously(call, recordingFrequency, recordingLength, audioClip);
        }

        private AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> GetAsyncDuplexStreamingCallOptions(ConvaiService.ConvaiServiceClient client)
        {
            Metadata headers = new()
            {
                { "source", "Unity" },
                { "version", "3.0.0" }

            };

            CallOptions options = new(headers);
            return client.GetResponse(options);
        }

        /// <summary>
        ///     Creates a GetResponseRequest object configured with the specified parameters for initiating a gRPC call.
        /// </summary>
        /// <param name="isActionActive">Indicates whether actions are enabled for the character.</param>
        /// <param name="isLipSyncActive">Indicates whether lip sync is enabled for the character.</param>
        /// <param name="recordingFrequency">The frequency at which the audio is recorded.</param>
        /// <param name="characterID">The unique identifier for the character.</param>
        /// <param name="actionConfig">The configuration for character actions.</param>
        /// <param name="faceModel">The facial model configuration for the character.</param>
        /// <returns>A GetResponseRequest object configured with the provided settings.</returns>
        private GetResponseRequest CreateGetResponseRequest(bool isActionActive, bool isLipSyncActive, int recordingFrequency, string characterID, ActionConfig actionConfig = null,
            FaceModel faceModel = FaceModel.OvrModelName)
        {
            GetResponseRequest getResponseConfigRequest = new()
            {
                GetResponseConfig = new GetResponseConfig
                {
                    CharacterId = characterID,
                    ApiKey = _apiKey, // Assumes apiKey is available
                    SessionId = _activeConvaiNPC.sessionID, // Assumes _activeConvaiNPC would not be null, else this will throw NullReferenceException

                    AudioConfig = new AudioConfig
                    {
                        SampleRateHertz = recordingFrequency,
                        EnableFacialData = isLipSyncActive,
                        FaceModel = faceModel
                    }
                }
            };

            if (isActionActive || _activeConvaiNPC != null) getResponseConfigRequest.GetResponseConfig.ActionConfig = actionConfig;

            return getResponseConfigRequest;
        }

        /// <summary>
        ///     Processes audio data continuously from a microphone input and sends it to the server via a gRPC call.
        /// </summary>
        /// <param name="call">The streaming call to send audio data to the server.</param>
        /// <param name="recordingFrequency">The frequency at which the audio is recorded.</param>
        /// <param name="recordingLength">The length of the audio recording in seconds.</param>
        /// <param name="audioClip">The AudioClip object that contains the audio data from the microphone.</param>
        /// <returns>A task that represents the asynchronous operation of processing and sending audio data.</returns>
        private async Task ProcessAudioContinuously(AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call, int recordingFrequency, int recordingLength,
            AudioClip audioClip)
        {
            // Run the receiving results from the server in the background without awaiting it here.
            Task receiveResultsTask = Task.Run(async () => { await ReceiveResultFromServer(call, _cancellationTokenSource.Token); }, _cancellationTokenSource.Token);

            int pos = 0;
            float[] audioData = new float[recordingFrequency * recordingLength];

            while (Microphone.IsRecording(MicrophoneManager.Instance.SelectedMicrophoneName))
            {
                await Task.Delay(200);
                int newPos = Microphone.GetPosition(MicrophoneManager.Instance.SelectedMicrophoneName);
                int diff = newPos - pos;

                if (diff > 0)
                {
                    if (audioClip == null)
                    {
                        try
                        {
                            _cancellationTokenSource?.Cancel();
                        }
                        catch (Exception e)
                        {
                            // Handle the Exception, which can occur if the CancellationTokenSource is already disposed. 
                            Logger.Warn("Exception when Audio Clip is null: " + e.Message,
                                Logger.LogCategory.Character);
                        }
                        finally
                        {
                            _cancellationTokenSource?.Dispose();
                            _cancellationTokenSource = null;
                            Logger.Info("The Cancellation Token Source was Disposed because the Audio Clip was empty.",
                                Logger.LogCategory.Character);
                        }

                        break;
                    }

                    audioClip.GetData(audioData, pos);
                    await ProcessAudioChunk(call, diff, audioData);
                    pos = newPos;
                }
            }

            // Process any remaining audio data.
            await ProcessAudioChunk(call,
                Microphone.GetPosition(MicrophoneManager.Instance.SelectedMicrophoneName) - pos,
                audioData).ConfigureAwait(false);

            await call.RequestStream.CompleteAsync();
        }

        /// <summary>
        ///     Stops recording and processing the audio.
        /// </summary>
        public void StopRecordAudio()
        {
            // End microphone recording
            Microphone.End(MicrophoneManager.Instance.SelectedMicrophoneName);

            try
            {
                Logger.Info(_activeConvaiNPC.characterName + " has stopped listening", Logger.LogCategory.Character);
                OnPlayerSpeakingChanged?.Invoke(false);
            }
            catch (Exception)
            {
                Logger.Error("No active NPC found", Logger.LogCategory.Character);
            }
        }

        /// <summary>
        ///     Processes each audio chunk and sends it to the server.
        /// </summary>
        /// <param name="call">gRPC Streaming call connecting to the getResponse function</param>
        /// <param name="diff">Length of the audio data from the current position to the position of the last sent chunk</param>
        /// <param name="audioData">Chunk of audio data that we want to be processed</param>
        private static async Task ProcessAudioChunk(AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call, int diff, IReadOnlyList<float> audioData)
        {
            if (diff > 0)
            {
                // Convert audio data to byte array
                byte[] audioByteArray = new byte[diff * sizeof(short)];

                for (int i = 0; i < diff; i++)
                {
                    float sample = audioData[i];
                    short shortSample = (short)(sample * short.MaxValue);
                    byte[] shortBytes = BitConverter.GetBytes(shortSample);
                    audioByteArray[i * sizeof(short)] = shortBytes[0];
                    audioByteArray[i * sizeof(short) + 1] = shortBytes[1];
                }

                // Send audio data to the gRPC server
                try
                {
                    await call.RequestStream.WriteAsync(new GetResponseRequest
                    {
                        GetResponseData = new GetResponseData
                        {
                            AudioData = ByteString.CopyFrom(audioByteArray)
                        }
                    });
                }
                catch (RpcException rpcException)
                {
                    if (rpcException.StatusCode == StatusCode.Cancelled)
                        Logger.Error(rpcException, Logger.LogCategory.Character);
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, Logger.LogCategory.Character);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="newActiveNPC"></param>
        public void InterruptCharacterSpeech(ConvaiNPC newActiveNPC)
        {
            // If the active NPC is speaking, cancel the ongoing gRPC call,
            // clear the response queue, and reset the character's speaking state, lip-sync, animation, and audio playback
            if (newActiveNPC != null)
            {
                // Cancel the ongoing gRPC call
                try
                {
                    _cancellationTokenSource?.Cancel();
                }
                catch (Exception e)
                {
                    // Handle the Exception, which can occur if the CancellationTokenSource is already disposed. 
                    Logger.Warn("Exception in Interrupt Character Speech: " + e.Message, Logger.LogCategory.Character);
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                    Logger.Info($"The Cancellation Token Source for {newActiveNPC} was Disposed in ConvaiGRPCAPI:InterruptCharacterSpeech.", Logger.LogCategory.Character);
                }

                _cancellationTokenSource = new CancellationTokenSource(); // Create a new token for future calls

                CharacterInterrupted?.Invoke();

                // Clear the response queue
                newActiveNPC.ClearResponseQueue();

                // Reset the character's speaking state
                newActiveNPC.SetCharacterTalking(false);

                // Stop any ongoing audio playback
                newActiveNPC.StopAllAudioPlayback();

                // Stop any ongoing lip sync for active NPC
                newActiveNPC.StopLipSync();

                // Reset the character's animation to idle
                newActiveNPC.ResetCharacterAnimation();
            }
        }

        /// <summary>
        ///     Periodically receives responses from the server and adds it to a static list in streaming NPC
        /// </summary>
        /// <param name="call">gRPC Streaming call connecting to the getResponse function</param>
        /// <param name="cancellationToken"></param>
        private async Task ReceiveResultFromServer(AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call, CancellationToken cancellationToken)
        {
            Queue<LipSyncBlendFrameData> lipSyncBlendFrameQueue = new();
            bool firstSilFound = false;
            while (!cancellationToken.IsCancellationRequested && await call.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                try
                {
                    // Get the response from the server
                    GetResponseResponse result = call.ResponseStream.Current;
                    OnResultReceived?.Invoke(result);

                    // Process different types of responses

                    if (result.UserQuery != null)
                        //if (_chatUIHandler != null)
                            // Add user query to the list
                            _stringUserText.Add(result.UserQuery.TextData);

                    // Trigger the current section of the narrative design manager in the active NPC
                    if (result.BtResponse != null) TriggerNarrativeSection(result);

                    // Add action response to the list in the active NPC
                    if (result.ActionResponse != null)
                        if (_activeConvaiNPC.actionsHandler != null)
                            _activeConvaiNPC.actionsHandler.actionResponseList.Add(result.ActionResponse.Action);

                    // Add audio response to the list in the active NPC
                    if (result.AudioResponse != null)
                    {
                        if (result.AudioResponse.AudioData != null)
                        {
                            // Add response to the list in the active NPC
                            if (result.AudioResponse.AudioData.ToByteArray().Length > 46)
                            {
                                byte[] wavBytes = result.AudioResponse.AudioData.ToByteArray();

                                // will only work for wav files
                                WavHeaderParser parser = new(wavBytes);
                                if (_activeConvaiNPC.convaiLipSync == null)
                                {
                                    Logger.DebugLog($"Enqueuing responses: {result.AudioResponse.TextData}", Logger.LogCategory.LipSync);
                                    _activeConvaiNPC.EnqueueResponse(result);
                                }
                                else
                                {
                                    LipSyncBlendFrameData.FrameType frameType =
                                        _activeConvaiNPC.convaiLipSync.faceModel == FaceModel.OvrModelName
                                            ? LipSyncBlendFrameData.FrameType.Visemes
                                            : LipSyncBlendFrameData.FrameType.Blendshape;
                                    lipSyncBlendFrameQueue.Enqueue(
                                        new LipSyncBlendFrameData(
                                            (int)(parser.CalculateDurationSeconds() * 30),
                                            result,
                                            frameType
                                        )
                                    );
                                }
                            }

                            // Check if the response contains visemes data and the active NPC has a LipSync component
                            if (result.AudioResponse.VisemesData != null)
                                if (_activeConvaiNPC.convaiLipSync != null)
                                {
                                    // Logger.Info(result.AudioResponse.VisemesData, Logger.LogCategory.LipSync);
                                    if (result.AudioResponse.VisemesData.Visemes.Sil == -2 || result.AudioResponse.EndOfResponse)
                                    {
                                        if (firstSilFound) lipSyncBlendFrameQueue.Dequeue().Process(_activeConvaiNPC);
                                        firstSilFound = true;
                                    }
                                    else
                                    {
                                        lipSyncBlendFrameQueue.Peek().Enqueue(result.AudioResponse.VisemesData);
                                    }
                                }

                            // Check if the response contains blendshapes data and the active NPC has a LipSync component
                            if (result.AudioResponse.BlendshapesFrame != null)
                                if (_activeConvaiNPC.convaiLipSync != null)
                                {
                                    if (lipSyncBlendFrameQueue.Peek().CanProcess() || result.AudioResponse.EndOfResponse)
                                    {
                                        lipSyncBlendFrameQueue.Dequeue().Process(_activeConvaiNPC);
                                    }
                                    else
                                    {
                                        lipSyncBlendFrameQueue.Peek().Enqueue(result.AudioResponse.BlendshapesFrame);

                                        if (lipSyncBlendFrameQueue.Peek().CanPartiallyProcess()) lipSyncBlendFrameQueue.Peek().ProcessPartially(_activeConvaiNPC);
                                    }
                                }
                        }

                        //
                        if (result.AudioResponse == null && result.DebugLog != null)
                            _activeConvaiNPC.EnqueueResponse(call.ResponseStream.Current);

                        // Check if the session id of active NPC is -1 then only update it
                        if (_activeConvaiNPC.sessionID == "-1")
                            // Update session ID in the active NPC
                            _activeConvaiNPC.sessionID = call.ResponseStream.Current.SessionId;
                    }
                }
                catch (RpcException rpcException)
                {
                    // Handle RpcExceptions, log or throw if necessary
                    if (rpcException.StatusCode == StatusCode.Cancelled)
                        Logger.Error(rpcException, Logger.LogCategory.Character);
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    Logger.DebugLog(ex, Logger.LogCategory.Character);
                }


            if (cancellationToken.IsCancellationRequested) await call.RequestStream.CompleteAsync();
        }

        /// <summary>
        /// </summary>
        /// <param name="result"></param>
        private void TriggerNarrativeSection(GetResponseResponse result)
        {
            // Trigger the current section of the narrative design manager in the active NPC
            if (result.BtResponse != null)
            {
                Debug.Log("Narrative Design SectionID: " + result.BtResponse.NarrativeSectionId);
                // Get the NarrativeDesignManager component from the active NPC
                NarrativeDesignManager narrativeDesignManager = _activeConvaiNPC.narrativeDesignManager;
                if (narrativeDesignManager != null)
                    MainThreadDispatcher.Instance.RunOnMainThread(() => { narrativeDesignManager.UpdateCurrentSection(result.BtResponse.NarrativeSectionId); });
                else
                    Debug.Log("NarrativeDesignManager component not found in the active NPC");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="characterID"></param>
        /// <param name="triggerConfig"></param>
        public async Task SendTriggerData(ConvaiService.ConvaiServiceClient client, string characterID, TriggerConfig triggerConfig)
        {
            AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call = GetAsyncDuplexStreamingCallOptions(client);

            GetResponseRequest getResponseConfigRequest = CreateGetResponseRequest(true, true, 0, characterID);

            try
            {
                await call.RequestStream.WriteAsync(getResponseConfigRequest);
                await call.RequestStream.WriteAsync(new GetResponseRequest
                {
                    GetResponseData = new GetResponseData
                    {
                        TriggerData = triggerConfig
                    }
                });
                await call.RequestStream.CompleteAsync();

                // Store the task that receives results from the server.
                Task receiveResultsTask = Task.Run(
                    async () => { await ReceiveResultFromServer(call, _cancellationTokenSource.Token); },
                    _cancellationTokenSource.Token);

                // Await the task if needed to ensure it completes before this method returns [OPTIONAL]
                await receiveResultsTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, Logger.LogCategory.Character);
            }
        }

        /// <summary>
        ///     Asynchronously sends feedback to the server.
        /// </summary>
        /// <param name="thumbsUp">Indicates whether the feedback is a thumbs up or thumbs down.</param>
        /// <param name="interactionID">The ID associated with the interaction.</param>
        /// <param name="feedbackText">The text content of the feedback.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SendFeedback(bool thumbsUp, string interactionID, string feedbackText)
        {
            // Create a FeedbackRequest object with the provided parameters.
            FeedbackRequest request = new()
            {
                InteractionId = interactionID,
                CharacterId = _activeConvaiNPC.characterID,
                SessionId = _activeConvaiNPC.sessionID,
                TextFeedback = new FeedbackRequest.Types.Feedback
                {
                    FeedbackText = feedbackText,
                    ThumbsUp = thumbsUp
                }
            };

            try
            {
                // Send the feedback request asynchronously and await the response.
                FeedbackResponse response = await _activeConvaiNPC.GetClient().SubmitFeedbackAsync(request, cancellationToken: _cancellationTokenSource.Token);

                // Log the feedback response.
                Logger.Info(response.FeedbackResponse_, Logger.LogCategory.Character);
            }
            catch (RpcException rpcException)
            {
                // Log an exception if there is an error in sending the feedback.
                Logger.Exception(rpcException, Logger.LogCategory.Character);
            }
        }

        #region Events

        public event Action CharacterInterrupted; // Event to notify when the character's speech is interrupted
        public event Action<GetResponseResponse> OnResultReceived; // Event to notify when a response is received from the server
        public event Action<bool> OnPlayerSpeakingChanged; // Event to notify when the player starts or stops speaking

        #endregion
    }
}