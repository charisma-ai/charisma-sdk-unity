# Charisma SDK for Unity

This Unity plugin is verified to work with projects using Unity version `2021.3` only. If you find the plugin also works in another version, feel free to submit a pull request to update this!

If you have any questions or need a hand, please reach out to us at [hello@charisma.ai](mailto:hello@charisma.ai)!

## Getting started

**Important:** Before setting up the Charisma SDK for Unity, you’ll need to have created a web or game story (not an app story!), which requires a Charisma licence. Please visit the [licencing docs on our website](https://charisma.ai/docs/licencing) for more info!

If you haven’t already, go ahead and create an Unity project. Make sure you are using a version of the engine that is supported by this plugin.

## Importing / Installing

Download the contents of this repo, extract it to a folder of your choice and then import the folder to your Unity Project.

## Usage

To create playthrough tokens, you’ll first need to find out your story ID, and optionally an API key and version of the story you want to play.

#### Story Id

The `StoryId` is the unique ID of the story that you want to play. To find this, navigate to your story on the Charisma website, and copy the ID number after `/stories` in the URL.

![StoryId](https://i.ibb.co/TcxRM8J/story-id.png)

Pass this in the `CharismaTokenSettings` object.

#### Story Version

This is the version of the story you want to play.

- To play the latest published version, keep this at 0.

- To play a specific, published, version of your story, set the number to that particular version.

- To play the draft version, set the number to -1. To do this, you must also supply your API key.

Pass this in the `CharismaTokenSettings` object.

#### API key

An `apiKey` should now be used for authentication for playthrough token creation instead of `draftToken`. This is now the recommended way to authenticate as API keys do not expire (unless regenerated) and are more secure than the `draftToken` if compromised. `draftToken `should no longer be used. However, please make sure to not share the API key with anyone you do not trust, and strip the key from any public builds as before.

![API key](https://i.ibb.co/X86bNVK/API-key.png)

#### Speech Options

The output format of the audio received from Charisma. The default audio format is Ogg. Unless there are specific requirements that mean the format has to be Wav, it is recommended to keep it as Ogg.

Audio can be received both as an audio clip (buffer) and a URL.

Pass this object in the Start function whenever you start a playthrough or in a Reply / Tap whenever you wish to change the type of audio you receive.

**IMPORTANT:** Wav files can only be generated on Windows. Having this option selected on any other platform will result in an error.

## Example

A small example has been created to demonstrate how the startup flow works. To test it out, create a story and run the example scene in the `example` folder.

The script itself can be found in the `example` folder as well.

## Known Issues

- Some character voices (those provided by Google) do not convert to audio files properly, resulting in an FMOD error. Please avoid using these voices for the time being.
