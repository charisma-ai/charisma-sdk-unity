# charisma-sdk-unity
Unity SDK for Charisma.ai

If you have any questions or issues, please contact me at: oscar@charisma.ai

### 1. Importing / Installing

* Download the contents of this repo, extract it to a folder of your choice and then import the folder to your Unity Project. 

8 If you get errors concerning Newtonsoft's Json Serializer, import the latest Newtonsoft.Json package from NuGet into your project.

### 2. Important Settings 

The following sections describe some the most important properties you will encounter using the SDK.

  **2.a StoryId**
  
  The Id of the story that you want to play. Each story has a unique Id. Navigate to the Charisma website, click the story you want to play from Unity. The story id can be found in the URL in the following location: 
  "https://charisma.ai/stories/>storyId<". 
  
  Pass this in the CharismaTokenSettings object.
  
  ![StoryId](https://i.ibb.co/sPqS9n2/StoryId.png)
  
  * **2.b Story Version**
  
  This is the version of the story you want to play. 
  
  * To play the latest published version, keep this at 0. 
  
  * To play a specific, published, version of your story, set the number to that particular version. 
  
  * To play the draft version, set the number to -1. To do this, you must also supply a Debug Token. More on how to get your Debug Token below. 
  
   Pass this in the CharismaTokenSettings object.
  
  **2.c Speech Options**
  
  The output format of the audio received from Charisma. the default audio format is Ogg. 
  Unless there are specific requirements that mean the format has to be Wav, I recommend keeping it as Ogg. 
  
  Audio can be received both as an audio clip (buffer) and a URL.
  
  Pass this object in the Start function whenever you start a play-through or in a Reply / Tap whenever you wish to change the type of audio you receive.
  
  **IMPORTANT:** Wav files can only be generated on Windows. Having this option selected on any other platform will result in an error.
  
  **2.d Debug Token**
  
  If you want to debug your draft version (Unpublished), the Story Version needs to be set to -1. 
  You also need to retrieve your token from the Charisma website. To do this, go the Charisma website, right-click to inspect the site,
  navigate to storage and copy the access token to the "Debug token"-field.
  
   Pass this in the CharismaTokenSettings object.
  
  **IMPORTANT:** This token renews every month or so. If you are having trouble accessing your draft version, check if the token has been renewed.
  
  ![Token](https://i.ibb.co/hfJk0H7/Token.png)
  
## 3. Example

I've created a small example for you to be able to understand how the startup flow works. To test it out, create as story and run the example scene. 
The script itself can be found in the example folder as well.

## 4. Known Issues (v0.02)

* Some character voices do not convert to audio files properly, resulting in an FMOD error. The voices concerned are all named after Game of Thrones characters. Please avoid using these voices for the time being. 