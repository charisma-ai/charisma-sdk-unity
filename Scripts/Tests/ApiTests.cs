using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace CharismaSDK.Tests
{
    [TestFixture]
    public class ApiTests
    {
        #region Test Variables

        // Pre-made story metadata, used to inform the tests
        private const int TEST_CASE_TOKENID = 15452;
        private const int TEST_CASE_VERSIONID = -1;
        private const string TEST_CASE_APIKEY = "cff59833-71fc-42c4-9077-220d42aef65a";

        private const string EXPECTED_REPLY_FROM_ACTOR = "Howdy!";

        private const string DRAFT_MEMORY_RECALL_VALUE = "delicious";
        private const string MEMORY_SAVE_VALUE_TO_ASSIGN = "banana";

        #endregion

        private string _conversationUuid;
        private string _tokenId;
        private Playthrough _charisma;

        private CreatePlaythroughTokenParams _playthroughTokenParams;

        [OneTimeSetUp]
        public void Setup()
        {
            _playthroughTokenParams = new CreatePlaythroughTokenParams(TEST_CASE_TOKENID, TEST_CASE_VERSIONID, TEST_CASE_APIKEY);
        }

        [SetUp]
        public void RepeatSetup()
        {
            _conversationUuid = null;
            _tokenId = null;
            _charisma = default;
        }

        [TearDown]
        public void Teardown()
        {
        }

        #region Tests

        [Category("API")]
        [UnityTest]
        public IEnumerator LoadPlaythrough()
        {
            string resultUuid = null;
            string resultToken = null;

            var task = CharismaAPI.CreatePlaythroughToken(_playthroughTokenParams, callback: (response) =>
            {
                // assign token and uuid post callback
                resultUuid = response.PlaythroughUuid;
                resultToken = response.Token;
            }
            );

            yield return task;

            Assert.AreNotEqual(resultUuid, null, "Resulting Uid is null. Callback did not get called? Playthrough request failed.");
            Assert.AreNotEqual(resultToken, null, "Resulting Uid is null. Callback did not get called? Playthrough request failed.");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator GetMessageHistory()
        {
            GetMessageHistoryResponse messageHistory = default;

            IEnumerator playthroughCreationCallback = default;
            IEnumerator getMessageHistory = default;

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(_playthroughTokenParams, callback: (tokenResponse) =>
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
                    });
                });
            });

            yield return createToken;
        
            // adding padding to accomodate for server response waiting time
            yield return StartAndWaitUntil(playthroughCreationCallback, () =>
            {
                return _charisma.IsReadyToPlay;
            });

            // send conversation reply
            _charisma.Reply(_conversationUuid, "Hi");

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
            Assert.IsTrue(messageHistory.Messages[0].message.text == EXPECTED_REPLY_FROM_ACTOR, null, "Expected message does not match!");
        }

        [Category("API")]
        [UnityTest]
        public IEnumerator GetPlaythroughInfo()
        {
            GetPlaythroughInfoResponse playthroughInfo = default;

            IEnumerator getPlaythroughInfoCall = default;

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(_playthroughTokenParams, callback: (tokenResponse) =>
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
        public IEnumerator SetMemorySingle()
        {
            IEnumerator setMemory = default;
            IEnumerator getPlaythroughInfoPostCall = default;

            string memoryRecallValue = DRAFT_MEMORY_RECALL_VALUE;
            string memorySaveValue = MEMORY_SAVE_VALUE_TO_ASSIGN;

            bool memoryChangePerformedSuccesfully = false;

            IEnumerator createToken = CharismaAPI.CreatePlaythroughToken(_playthroughTokenParams, callback: (tokenResponse) =>
            {
                setMemory = CharismaAPI.SetMemory(tokenResponse.Token, memoryRecallValue, memorySaveValue, callback: () =>
                {
                    getPlaythroughInfoPostCall = CharismaAPI.GetPlaythroughInfo(tokenResponse.Token, callback: (playthroughInfoResult) =>
                    {
                        // check that the memories were set correctly
                        foreach(var memory in playthroughInfoResult.Memories)
                        {
                            if(memory.recallValue == memoryRecallValue)
                            {
                                if(memory.saveValue == memorySaveValue)
                                {
                                    memoryChangePerformedSuccesfully = true;
                                }
                            }

                        }

                    }
                    );
                }
                );
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
        public IEnumerator ForkPlaythroughToken()
        {
            IEnumerator forkPlaythrough = default;
            IEnumerator playthroughCreationCallback = default;
            IEnumerator getDraftPlaythroughInfoCall = default;
            IEnumerator getForkedPlaythroughInfoCall = default;

            GetPlaythroughInfoResponse draftPlaythroughInfo = default;
            GetPlaythroughInfoResponse forkedPlaythroughInfo = default;

            IEnumerator createPlaythroughToken = CharismaAPI.CreatePlaythroughToken(_playthroughTokenParams, callback: (tokenResponse) =>
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
            yield return StartAndWaitUntil(createPlaythroughToken, () =>
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
            Assert.AreNotEqual(draftPlaythroughInfo.Emotions.Length, forkedPlaythroughInfo.Emotions.Length, message: "Draft and Forked playthrough have the same count, which is not expected.");
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

        #endregion

    }
}
