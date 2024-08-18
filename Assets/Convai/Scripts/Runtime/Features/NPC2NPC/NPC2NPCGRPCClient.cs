﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Convai.Scripts.Utils.LipSync;
using Grpc.Core;
using Service;
using UnityEngine;

namespace Convai.Scripts.Utils
{
    /// <summary>
    ///     Represents an NPC2NPCGRPCClient that can be used to communicate with the ConvaiService using gRPC.
    /// </summary>
    public class NPC2NPCGRPCClient : MonoBehaviour
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private string _apiKey;
        private ConvaiService.ConvaiServiceClient _client;
        private NPCGroup _npcGroup;

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        ///     Initializes the NPC2NPCGRPCClient with the given API key and ConvaiService client.
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <param name="client">The ConvaiService client to use for communication.</param>
        public void Initialize(string apiKey, ConvaiService.ConvaiServiceClient client, NPCGroup group)
        {
            _apiKey = apiKey;
            _client = client;
            _npcGroup = group;
        }

        /// <summary>
        ///     Creates an AsyncDuplexStreamingCall with the specified headers.
        /// </summary>
        /// <returns>An AsyncDuplexStreamingCall with the specified headers.</returns>
        private AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> GetAsyncDuplexStreamingCallOptions()
        {
            Metadata headers = new()
            {
                { "source", "Unity" },
                { "version", "3.0.0" }
            };

            CallOptions options = new(headers);
            return _client.GetResponse(options);
        }

        /// <summary>
        ///     Sends the specified user text to the server and receives a response.
        /// </summary>
        /// <param name="userText">The user text to send to the server.</param>
        /// <param name="characterID">The ID of the character to use for the request.</param>
        /// <param name="sessionID">The ID of the session to use for the request.</param>
        /// <param name="isLipSyncActive">Whether lip sync is active for the request.</param>
        /// <param name="faceModel">The face model to use for the request.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SendTextData(string userText, string characterID, string sessionID, bool isLipSyncActive, FaceModel faceModel)
        {
            AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call = GetAsyncDuplexStreamingCallOptions();

            GetResponseRequest getResponseConfigRequest = CreateGetResponseRequest(characterID, sessionID, isLipSyncActive, faceModel, false, null);

            try
            {
                await call.RequestStream.WriteAsync(getResponseConfigRequest);
                await call.RequestStream.WriteAsync(new GetResponseRequest
                {
                    GetResponseData = new GetResponseRequest.Types.GetResponseData
                    {
                        TextData = userText
                    }
                });
                await call.RequestStream.CompleteAsync();

                Task receiveResultsTask = Task.Run(
                    async () => { await ReceiveResultFromServer(call, _cancellationTokenSource.Token); },
                    _cancellationTokenSource.Token);
                await receiveResultsTask.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Creates a GetResponseRequest with the specified parameters.
        /// </summary>
        /// <param name="characterID">The ID of the character to use for the request.</param>
        /// <param name="sessionID">The ID of the session to use for the request.</param>
        /// <param name="isLipSyncActive">Whether lip sync is active for the request.</param>
        /// <param name="faceModel">The face model to use for the request.</param>
        /// <param name="isActionActive">Whether action is active for the request.</param>
        /// <param name="actionConfig">The action configuration to use for the request.</param>
        /// <returns>A GetResponseRequest with the specified parameters.</returns>
        private GetResponseRequest CreateGetResponseRequest(string characterID, string sessionID, bool isLipSyncActive, FaceModel faceModel, bool isActionActive,
            ActionConfig actionConfig)
        {
            GetResponseRequest getResponseConfigRequest = new()
            {
                GetResponseConfig = new GetResponseRequest.Types.GetResponseConfig
                {
                    CharacterId = characterID,
                    ApiKey = _apiKey,
                    SessionId = sessionID,
                    AudioConfig = new AudioConfig
                    {
                        EnableFacialData = isLipSyncActive,
                        FaceModel = faceModel
                    }
                }
            };

            if (isActionActive)
                getResponseConfigRequest.GetResponseConfig.ActionConfig = actionConfig;

            return getResponseConfigRequest;
        }

        /// <summary>
        ///     Receives a response from the server asynchronously.
        /// </summary>
        /// <param name="call">The AsyncDuplexStreamingCall to use for receiving the response.</param>
        /// <param name="cancellationToken">The cancellation token to use for cancelling the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ReceiveResultFromServer(AsyncDuplexStreamingCall<GetResponseRequest, GetResponseResponse> call, CancellationToken cancellationToken)
        {
            Logger.Info("Receiving response from server", Logger.LogCategory.Character);
            ConvaiNPC convaiNPC = _npcGroup.CurrentSpeaker.ConvaiNPC;
            _npcGroup.CurrentSpeaker.CanRelayMessage = true;
            Queue<LipSyncBlendFrameData> lipSyncBlendFrameQueue = new Queue<LipSyncBlendFrameData>();
            bool firstSilFound = false;
            while (!cancellationToken.IsCancellationRequested && await call.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                try
                {
                    GetResponseResponse result = call.ResponseStream.Current;
                    // Process the received response here
                    if (result.AudioResponse != null)
                    {
                        if (result.AudioResponse.AudioData != null)
                        // Add response to the list in the active NPC
                        {
                            if (result.AudioResponse.AudioData.ToByteArray().Length > 46)
                            {
                                byte[] wavBytes = result.AudioResponse.AudioData.ToByteArray();

                                // will only work for wav files
                                WavHeaderParser parser = new(wavBytes);
                                Logger.DebugLog($"Enqueuing responses: {result.AudioResponse.TextData}", Logger.LogCategory.LipSync);
                                convaiNPC.EnqueueResponse(result);
                            }


                            if (result.AudioResponse.EndOfResponse)
                            {
                                MainThreadDispatcher.Instance.RunOnMainThread(() =>
                                {
                                    _npcGroup.CurrentSpeaker.EndOfResponseReceived();
                                });
                            }
                        }
                    }
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
                    Logger.DebugLog(ex, Logger.LogCategory.Character);
                }
        }
    }
}