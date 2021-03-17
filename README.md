# Unity SDK for Charisma.ai

If you have any questions or issues, please contact us at: oscar@charisma.ai

## Importing / Installing

Download the contents of this repo, extract it to a folder of your choice and then import the folder to your Unity Project.

If you are getting errors from the Newtonsoft Json Serializer, import the latest Newtonsoft.Json package from NuGet into your project.

## Important Settings

The following sections describe usage of the SDK.

#### Story Id

The unique id of the story that you want to play. To find this, navigate to the Charisma website and click the story you want to play from Unity. The story id can be found in the URL in the following location:
"https://charisma.ai/stories/:storyId".

Pass this in the `CharismaTokenSettings` object.

![StoryId](https://i.ibb.co/sPqS9n2/StoryId.png)

#### Story Version

This is the version of the story you want to play.

- To play the latest published version, keep this at 0.

- To play a specific, published, version of your story, set the number to that particular version.

- To play the draft version, set the number to -1. To do this, you must also supply your API key.

Pass this in the `CharismaTokenSettings` object.

#### Speech Options

The output format of the audio received from Charisma. The default audio format is Ogg. Unless there are specific requirements that mean the format has to be Wav, it is recommended to keep it as Ogg.

Audio can be received both as an audio clip (buffer) and a URL.

Pass this object in the Start function whenever you start a playthrough or in a Reply / Tap whenever you wish to change the type of audio you receive.

**IMPORTANT:** Wav files can only be generated on Windows. Having this option selected on any other platform will result in an error.

#### API key

An `apiKey` should now be used for authentication for playthrough token creation instead of `draftToken`. This is now the recommended way to authenticate as API keys do not expire (unless regenerated) and are more secure than the `draftToken` if compromised. `draftToken `should no longer be used. However, please make sure to not share the API key with anyone you do not trust, and strip the key from any public builds as before.

![API key](https://i.ibb.co/X86bNVK/API-key.png)

## Example

A small example has been created to demonstrate how the startup flow works. To test it out, create a story and run the example scene.

The script itself can be found in the example folder as well.

## Known Issues

- Some character voices do not convert to audio files properly, resulting in an FMOD error. The voices concerned are all named after Game of Thrones characters. Please avoid using these voices for the time being.
