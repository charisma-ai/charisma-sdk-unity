#if UNITY_EDITOR

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace CharismaSDK.Tests
{
    [TestFixture]
    public class ApiTests
    {
        private string _conversationUuid;
        private Playthrough _charisma;

        [SetUp]
        public void RepeatSetup()
        {
            _conversationUuid = null;
            _charisma = default;
        }

        [TearDown]
        public void Teardown()
        {
        }

        #region Tests

        [Category("API")]
        [UnityTest]
        public IEnumerator CreatePlaythroughToken([ValueSource("GetApiTestData")] CharismaApiTestData testParameters)
        {
            string resultUuid = null;
            string resultToken = null;

            var playthroughToken = new CreatePlaythroughTokenParams(testParameters.StoryId, testParameters.StoryVersion, testParameters.ApiKey);

            var task = CharismaAPI.CreatePlaythroughToken(playthroughToken, callback: (response) =>
            {
                // assign token and uuid post callback
                resultUuid = response.PlaythroughUuid;
                resultToken = response.Token;
            }
            );

            yield return task;

            Assert.AreNotEqual(resultUuid, null, "Resulting uuid is null. Callback did not get called? Playthrough request failed.");
            Assert.AreNotEqual(resultToken, null, "Resulting uuid is null. Callback did not get called? Playthrough request failed.");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator GetMessageHistory([ValueSource("GetApiTestData")] CharismaApiTestData testParameters)
        {
            bool hasConnected = false;
            GetMessageHistoryResponse messageHistory = default;

            IEnumerator playthroughCreationCallback = default;
            IEnumerator getMessageHistory = default;

            var playthroughToken = new CreatePlaythroughTokenParams(testParameters.StoryId, testParameters.StoryVersion, testParameters.ApiKey);

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(playthroughToken, callback: (tokenResponse) =>
            {
                playthroughCreationCallback = CharismaAPI.CreateConversation(token: tokenResponse.Token, callback: conversationUuid =>
                {
                    _conversationUuid = conversationUuid;

                    _charisma = new Playthrough(
                        tokenResponse.Token,
                        tokenResponse.PlaythroughUuid
                    );

                    // hook up on message callback before attempting to connect
                    _charisma.OnMessage += (message) =>
                    {
                        getMessageHistory = CharismaAPI.GetMessageHistory(tokenResponse.Token, _conversationUuid, null, callback: (messageHistoryResponse) =>
                        {
                            messageHistory = messageHistoryResponse;
                        }
                        );
                    };

                    _charisma.Connect(onReadyCallback: () =>
                    {
                        _charisma.Start(_conversationUuid);
                        hasConnected = true;
                    });
                });
            });

            yield return createToken;
        
            // adding padding to accomodate for server response waiting time
            yield return StartAndWaitUntil(playthroughCreationCallback, () =>
            {
                return hasConnected;
            });

            // send conversation reply
            _charisma.Reply(_conversationUuid, testParameters.FirstReply);

            // need to wait for reply to be handled and message received callback to be received
            yield return WaitUntil(() =>
            {
                return getMessageHistory != default;
            });

            // adding padding to accomodate for message history callback
            yield return StartAndWaitUntil(getMessageHistory, () =>
            {
                return messageHistory != default;
            });

            Assert.IsTrue(messageHistory != default, "Message History was never returned");
            Assert.IsNotEmpty(messageHistory.Messages, null, "Message history is empty.");
            Assert.IsTrue(messageHistory.Messages[0].message.text == testParameters.FirstExpectedMessage, null, "Expected message does not match!");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator GetPlaythroughInfo([ValueSource("GetApiTestData")] CharismaApiTestData testParameters)
        {
            GetPlaythroughInfoResponse playthroughInfo = default;

            IEnumerator getPlaythroughInfoCall = default;

            var playthroughToken = new CreatePlaythroughTokenParams(testParameters.StoryId, testParameters.StoryVersion, testParameters.ApiKey);

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(playthroughToken, callback: (tokenResponse) =>
            {
                getPlaythroughInfoCall = CharismaAPI.GetPlaythroughInfo(tokenResponse.Token, callback: (playthroughInfoResult) =>
                {
                    playthroughInfo = playthroughInfoResult;
                }
                );
            });

            // adding padding to accomodate for token generation
            yield return StartAndWaitUntil(createToken, () =>
            {
                return getPlaythroughInfoCall != default;
            });

            yield return getPlaythroughInfoCall;

            Assert.IsTrue(playthroughInfo != default, "Playthrough Info was never returned.");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator SetMemorySingle([ValueSource("GetApiTestData")] CharismaApiTestData testParameters)
        {
            IEnumerator setMemory = default;
            IEnumerator getPlaythroughInfoPostCall = default;

            if (!testParameters.HasMemoryValue)
            {
                Assert.Pass("No Memory Value has been set, skipping this test");
            }

            string memoryRecallValue = testParameters.MemoryRecallValue;
            string memorySaveValue = testParameters.MemorySaveValue;

            bool memoryChangePerformedSuccesfully = false;

            var playthroughToken = new CreatePlaythroughTokenParams(testParameters.StoryId, testParameters.StoryVersion, testParameters.ApiKey);

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(playthroughToken, callback: (tokenResponse) =>
            {
                setMemory = CharismaAPI.SetMemory(tokenResponse.Token, memoryRecallValue, memorySaveValue, callback: () =>
                {
                    getPlaythroughInfoPostCall = CharismaAPI.GetPlaythroughInfo(tokenResponse.Token, callback: (playthroughInfoResult) =>
                    {
                        // check that the memories were set correctly
                        foreach (var memory in playthroughInfoResult.Memories)
                        {
                            if (memory.recallValue == memoryRecallValue
                            && memory.saveValue == memorySaveValue)
                            {
                                memoryChangePerformedSuccesfully = true;
                            }
                        }
                    });
                });
            });

            // adding padding to accomodate for token generation
            yield return StartAndWaitUntil(createToken, () =>
            {
                return setMemory != default;
            });

            // adding padding to accomodate for connection latency
            yield return StartAndWaitUntil(setMemory, () =>
            {
                return getPlaythroughInfoPostCall != default;
            });

            yield return getPlaythroughInfoPostCall;

            Assert.IsTrue(memoryChangePerformedSuccesfully, "Memory was not set correctly.");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator ForkPlaythroughToken([ValueSource("GetApiTestData")] CharismaApiTestData testParameters)
        {
            IEnumerator forkPlaythrough = default;
            IEnumerator playthroughCreationCallback = default;
            IEnumerator getDraftPlaythroughInfoCall = default;
            IEnumerator getForkedPlaythroughInfoCall = default;

            GetPlaythroughInfoResponse draftPlaythroughInfo = default;
            GetPlaythroughInfoResponse forkedPlaythroughInfo = default;

            if (!testParameters.HasPublishedPlaythroughForFork)
            {
                Assert.Pass("No Published playthrough available to fork to, skipping this test");
            }

            var playthroughToken = new CreatePlaythroughTokenParams(testParameters.StoryId, testParameters.StoryVersion, testParameters.ApiKey);

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(playthroughToken, callback: (tokenResponse) =>
            {
                playthroughCreationCallback = CharismaAPI.CreateConversation(token: tokenResponse.Token, callback: conversationUuid =>
                {
                    _conversationUuid = conversationUuid;

                    _charisma = new Playthrough(
                        tokenResponse.Token,
                        tokenResponse.PlaythroughUuid
                    );

                    _charisma.Connect(onReadyCallback: () =>
                    {
                        _charisma.Start(_conversationUuid);

                        forkPlaythrough = CharismaAPI.ForkPlaythroughToken(tokenResponse.Token, callback: (response) =>
                        {
                            getForkedPlaythroughInfoCall = CharismaAPI.GetPlaythroughInfo(response.NewToken, callback: (result) =>
                            {
                                forkedPlaythroughInfo = result;
                            });
                        });
                    });
                });

                getDraftPlaythroughInfoCall = CharismaAPI.GetPlaythroughInfo(tokenResponse.Token, callback: (playthroughInfoResult) =>
                {
                    draftPlaythroughInfo = playthroughInfoResult;
                });

            });

            // adding padding to accomodate for token generation
            yield return StartAndWaitUntil(createToken, () =>
            {
                return playthroughCreationCallback != default;
            });

            yield return StartAndWaitUntil(playthroughCreationCallback, () =>
            {
                return getDraftPlaythroughInfoCall != default;
            });

            yield return StartAndWaitUntil(getDraftPlaythroughInfoCall, () =>
            {
                return forkPlaythrough != default;
            });

            yield return StartAndWaitUntil(forkPlaythrough, () =>
            {
                return getForkedPlaythroughInfoCall != default;
            });

            yield return getForkedPlaythroughInfoCall;

            // to identify succesful fork, draft version feature a single Actor
            // while forked version features two
            var actorCountDraft = draftPlaythroughInfo.Emotions.Length;
            var actorCountFork = forkedPlaythroughInfo.Emotions.Length;

            Assert.AreEqual(actorCountDraft, testParameters.DraftActorCount, message: "Number of actors provided in test Data mismatches from Draft's actual actor count");
            Assert.AreEqual(actorCountFork, testParameters.ForkActorCount, message: "Number of actors provided in test Data mismatches from Fork's actual actor count");
            Assert.AreNotEqual(actorCountDraft, actorCountFork, message: "Draft and Forked playthrough have the same emotion count, which is not expected.");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator ResetPlaythrough([ValueSource("GetApiTestData")] CharismaApiTestData testParameters)
        {
            GetMessageHistoryResponse messageHistory = default;
            GetMessageHistoryResponse postResetMessageHistory = default;

            IEnumerator playthroughCreationCallback = default;
            IEnumerator getMessageHistoryPreReset = default;
            IEnumerator resetPlaythrough = default;
            IEnumerator getMessageHistoryPostReset = default;

            bool triggeredOnMessageCallback = false;
            long firstMessageId = 0;
            bool hasConnected = false;

            var playthroughToken = new CreatePlaythroughTokenParams(testParameters.StoryId, testParameters.StoryVersion, testParameters.ApiKey);

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(playthroughToken, callback: (tokenResponse) =>
            {
                playthroughCreationCallback = CharismaAPI.CreateConversation(token: tokenResponse.Token, callback: conversationUuid =>
                {
                    _conversationUuid = conversationUuid;

                    _charisma = new Playthrough(
                        tokenResponse.Token,
                        tokenResponse.PlaythroughUuid
                    );

                    // hook up on message callback before attempting to connect
                    _charisma.OnMessage += (message) =>
                    {
                        triggeredOnMessageCallback = true;

                        // expecting 2nd reply, before calling any of the getter messages
                        if (message.message.text != testParameters.SecondExpectedMessage)
                        {
                            firstMessageId = message.eventId;
                            return;
                        }

                        getMessageHistoryPreReset = CharismaAPI.GetMessageHistory(tokenResponse.Token, _conversationUuid, null, callback: (messageHistoryResponse) =>
                        {
                            messageHistory = messageHistoryResponse;
                        }
                        );

                        resetPlaythrough = CharismaAPI.ResetPlaythrough(tokenResponse.Token, firstMessageId, () =>
                        {
                            getMessageHistoryPostReset = CharismaAPI.GetMessageHistory(tokenResponse.Token, _conversationUuid, null, callback: (response) =>
                            {
                                postResetMessageHistory = response;
                            }
                        );
                        });
                    };

                    _charisma.Connect(onReadyCallback: () =>
                    {
                        _charisma.Start(_conversationUuid);
                        hasConnected = true;
                    });
                });
            });

            yield return createToken;

            // adding padding to accomodate for server response waiting time
            yield return StartAndWaitUntil(playthroughCreationCallback, () =>
            {
                return hasConnected;
            });

            // send conversation reply
            _charisma.Reply(_conversationUuid, testParameters.FirstReply);

            yield return WaitUntil(() =>
            {
                return triggeredOnMessageCallback;
            });

            // send 2nd reply
            _charisma.Reply(_conversationUuid, testParameters.SecondReply);

            // need to wait for reply to be handled and message received callback to be received
            yield return WaitUntil(() =>
            {
                return getMessageHistoryPreReset != default;
            });

            // adding padding to accomodate for message history callback
            yield return StartAndWaitUntil(getMessageHistoryPreReset, () =>
            {
                return messageHistory != default;
            });

            // all playthrough reset once we have message history
            yield return StartAndWaitUntil(resetPlaythrough, () =>
            {
                return getMessageHistoryPostReset != default;
            });

            // clear message callback flag, since after a reset we want to wait for the message callback to happen again
            triggeredOnMessageCallback = false;
            _charisma.Start(_conversationUuid);
            _charisma.Reply(_conversationUuid, testParameters.FirstReply);

            yield return WaitUntil(() =>
            {
                return triggeredOnMessageCallback;
            });

            // request post reset message history to confirm the state of the playthrough
            yield return StartAndWaitUntil(getMessageHistoryPostReset, () =>
            {
                return postResetMessageHistory != default;
            });

            Assert.IsTrue(messageHistory != default, "Pre-reset Message History was never returned");
            Assert.IsTrue(postResetMessageHistory != default, "Post-reset Message History was never returned");
            Assert.IsNotEmpty(messageHistory.Messages, null, "Pre-reset Message history is empty.");
            Assert.IsNotEmpty(postResetMessageHistory.Messages, null, "Post-reset Message history is empty.");
            Assert.AreNotEqual(messageHistory.Messages.Length, postResetMessageHistory.Messages.Length, "Total number of messages pre reset and post reset match, which is incorrect.");
        }

        #endregion

        #region Private Helpers
        static public IEnumerator StartAndWaitUntil(IEnumerator enumerator, Func<bool> condition, float timeout = 10f)
        {
            yield return enumerator;

            yield return WaitUntil(condition, timeout);
        }

        static public IEnumerator WaitUntil(Func<bool> condition, float timeout = 10f)
        {
            float timePassed = 0f;
            while (!condition() && timePassed < timeout)
            {
                yield return new WaitForEndOfFrame();
                timePassed += Time.deltaTime;
            }
            if (timePassed >= timeout)
            {
                throw new TimeoutException("Condition was not fulfilled for " + timeout + " seconds.");
            }
        }


        private static IEnumerable GetApiTestData

        {
            get
            {
                var contents = LoadTestFileContents();

                foreach (var entry in contents)
                {
                    yield return entry;
                }
            }
        }

        private static List<CharismaApiTestData> LoadTestFileContents()
        {
            List<CharismaApiTestData> dataset = new List<CharismaApiTestData>();

            var objects = AssetDatabase.FindAssets("t:CharismaApiTestData", new[] { "Assets/" });

            foreach (var guid in objects)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var loadedData = (CharismaApiTestData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(CharismaApiTestData));
                dataset.Add(loadedData);
            }

            return dataset;
        }

        #endregion

    }
}

#endif
